using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;
using DPFP;
using DPFP.Capture;
using DPFP.Processing;
using DPFP.Verification;

namespace BiometriaDP.Services
{
    public class FingerprintCapturedEventArgs : EventArgs
    {
        public byte[] TemplateBytes { get; }
        public FingerprintCapturedEventArgs(byte[] tpl) => TemplateBytes = tpl;
    }

    public class FingerprintVerifiedEventArgs : EventArgs
    {
        public int IdUsuario { get; }
        public bool IsAdmin { get; }
        public FingerprintVerifiedEventArgs(int id, bool isAdmin) { IdUsuario = id; IsAdmin = isAdmin; }
    }

    public class FingerprintService : DPFP.Capture.EventHandler, IDisposable
    {
        public event EventHandler<FingerprintCapturedEventArgs> OnEnrollmentComplete;
        public event EventHandler<FingerprintVerifiedEventArgs> OnVerificationComplete;
        public event EventHandler<string> OnError;

        private readonly string _connectionString;
        private readonly int _loggedAdminId;
        private Capture _capturador;
        private Verification _verificador;
        private Enrollment _enroller;
        private bool _enrollmentMode;
        private List<UserTemplate> _userTemplates;

        public FingerprintService(string connectionString, int loggedAdminId)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _loggedAdminId = loggedAdminId;

            // SDK initialization
            _capturador = new Capture { EventHandler = this };
            _verificador = new Verification();
            _enroller = new Enrollment();
            _enrollmentMode = false;

            // Load only templates belonging to this admin
            _userTemplates = LoadAllTemplates();
        }

        public void StartVerification()
        {
            try
            {
                _enrollmentMode = false;
                _capturador.StartCapture();
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, $"Error al iniciar verificación: {ex.Message}");
            }
        }

        public void StartEnrollment()
        {
            try
            {
                _enrollmentMode = true;
                _enroller.Clear();
                _capturador.StartCapture();
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, $"Error al iniciar registro de huella: {ex.Message}");
            }
        }

        public void OnComplete(object capture, string readerSerialNumber, Sample sample)
        {
            _capturador.StopCapture();
            if (_enrollmentMode) ProcessEnrollment(sample);
            else ProcessVerification(sample);
        }
        public void OnFingerTouch(object c, string r) { }
        public void OnFingerGone(object c, string r) { }
        public void OnReaderConnect(object c, string r) { }
        public void OnReaderDisconnect(object c, string r) { }
        public void OnSampleQuality(object c, string r, CaptureFeedback f) { }

        private void ProcessEnrollment(Sample sample)
        {
            var features = ExtractFeatures(sample, DataPurpose.Enrollment);
            if (features == null)
            {
                OnError?.Invoke(this, "Huella de mala calidad, intente de nuevo.");
                _capturador.StartCapture();
                return;
            }
            _enroller.AddFeatures(features);
            if (_enroller.TemplateStatus == Enrollment.Status.Ready)
            {
                byte[] tplBytes;
                using (var ms = new MemoryStream())
                {
                    _enroller.Template.Serialize(ms);
                    tplBytes = ms.ToArray();
                }
                OnEnrollmentComplete?.Invoke(this, new FingerprintCapturedEventArgs(tplBytes));
                _enrollmentMode = false;
            }
            else
            {
                int needed = (int)_enroller.FeaturesNeeded;
                OnError?.Invoke(this, $"Pasada recibida. Repita {needed} vez{(needed > 1 ? "es" : "")} más.");
                _capturador.StartCapture();
            }
        }

        private void ProcessVerification(Sample sample)
        {
            var features = ExtractFeatures(sample, DataPurpose.Verification);
            if (features == null) { OnError?.Invoke(this, "No se obtuvieron características válidas"); return; }

            int matchId = -1;
            bool matchIsAdmin = false;
            foreach (var ut in _userTemplates)
            {
                var tpl = new DPFP.Template();
                tpl.DeSerialize(ut.Template);
                var res = new DPFP.Verification.Verification.Result();
                _verificador.Verify(features, tpl, ref res);
                if (res.Verified)
                {
                    matchId = ut.Id;
                    matchIsAdmin = ut.IsAdmin;
                    break;
                }
            }
            OnVerificationComplete?.Invoke(this, new FingerprintVerifiedEventArgs(matchId, matchIsAdmin));
        }

        private FeatureSet ExtractFeatures(Sample sample, DataPurpose purpose)
        {
            var extractor = new FeatureExtraction();
            CaptureFeedback feedback = CaptureFeedback.None;
            var features = new FeatureSet();
            extractor.CreateFeatureSet(sample, purpose, ref feedback, ref features);
            return feedback == CaptureFeedback.Good ? features : null;
        }

        private List<UserTemplate> LoadAllTemplates()
        {
            var list = new List<UserTemplate>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                // Load only this admin's template
                using (var cmdA = new SqlCommand("SELECT id_admins, admin_huella FROM admins WHERE admin_huella IS NOT NULL AND id_admins = @adminId", conn))
                {
                    cmdA.Parameters.AddWithValue("@adminId", _loggedAdminId);
                    using (var rdrA = cmdA.ExecuteReader())
                        while (rdrA.Read())
                            list.Add(new UserTemplate(rdrA.GetInt32(0), rdrA.GetFieldValue<byte[]>(1), true));
                }
                // Load only this admin's employees
                using (var cmdE = new SqlCommand("SELECT id_empleado, emp_huella FROM empleado WHERE emp_huella IS NOT NULL AND id_admins = @adminId", conn))
                {
                    cmdE.Parameters.AddWithValue("@adminId", _loggedAdminId);
                    using (var rdrE = cmdE.ExecuteReader())
                        while (rdrE.Read())
                            list.Add(new UserTemplate(rdrE.GetInt32(0), rdrE.GetFieldValue<byte[]>(1), false));
                }
            }
            return list;
        }

        public void Dispose() => _capturador?.StopCapture();

        class UserTemplate
        {
            public int Id { get; }
            public byte[] Template { get; }
            public bool IsAdmin { get; }
            public UserTemplate(int id, byte[] tpl, bool isAdmin) { Id = id; Template = tpl; IsAdmin = isAdmin; }
        }
    }
}
