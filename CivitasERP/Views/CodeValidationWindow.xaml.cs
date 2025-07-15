using CivitasERP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CivitasERP.Views
{
    /// <summary>
    /// Lógica de interacción para CodeValidationWindow.xaml
    /// </summary>
    public partial class CodeValidationWindow : Window
    {
        private TextBox[] codeTextBoxes;

        public CodeValidationWindow()
        {
            InitializeComponent();
            InitializeCodeTextBoxes();
        }

        private void InitializeCodeTextBoxes()
        {
            // Inicializar array de TextBoxes
            codeTextBoxes = new TextBox[]
            {
                txtCodigo1, txtCodigo2, txtCodigo3,
                txtCodigo4, txtCodigo5, txtCodigo6
            };

            // Establecer el foco en el primer cuadro
            txtCodigo1.Focus();
        }

        // Validar que solo se ingresen números
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Solo permitir números
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        // Manejar el cambio de texto y navegación automática
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox currentTextBox = sender as TextBox;

            if (currentTextBox.Text.Length == 1)
            {
                // Buscar el índice del TextBox actual
                int currentIndex = Array.IndexOf(codeTextBoxes, currentTextBox);

                // Mover al siguiente TextBox si no es el último
                if (currentIndex < codeTextBoxes.Length - 1)
                {
                    codeTextBoxes[currentIndex + 1].Focus();
                }
                else
                {
                    // Si es el último cuadro, validar automáticamente
                    ValidateCode();
                }
            }
        }

        // Manejar teclas especiales (Backspace, Delete, etc.)
        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox currentTextBox = sender as TextBox;
            int currentIndex = Array.IndexOf(codeTextBoxes, currentTextBox);

            if (e.Key == Key.Back && currentTextBox.Text.Length == 0)
            {
                // Si presiona Backspace en un cuadro vacío, ir al anterior
                if (currentIndex > 0)
                {
                    codeTextBoxes[currentIndex - 1].Focus();
                    codeTextBoxes[currentIndex - 1].SelectAll();
                }
            }
            else if (e.Key == Key.Delete)
            {
                // Limpiar el cuadro actual
                currentTextBox.Clear();
            }
            else if (e.Key == Key.Left && currentIndex > 0)
            {
                // Mover a la izquierda
                codeTextBoxes[currentIndex - 1].Focus();
                codeTextBoxes[currentIndex - 1].SelectAll();
            }
            else if (e.Key == Key.Right && currentIndex < codeTextBoxes.Length - 1)
            {
                // Mover a la derecha
                codeTextBoxes[currentIndex + 1].Focus();
                codeTextBoxes[currentIndex + 1].SelectAll();
            }
            else if (e.Key == Key.Enter)
            {
                // Validar código al presionar Enter
                ValidateCode();
            }
        }

        // Seleccionar todo el texto al hacer foco
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            textBox.SelectAll();
        }

        // Obtener el código completo de los 6 cuadros
        private string GetVerificationCode()
        {
            return string.Join("", codeTextBoxes.Select(tb => tb.Text));
        }

        // Limpiar todos los cuadros
        private void ClearAllTextBoxes()
        {
            foreach (var textBox in codeTextBoxes)
            {
                textBox.Clear();
            }
            txtCodigo1.Focus();
        }

        // Validar código automáticamente cuando se completan los 6 dígitos
        private void ValidateCode()
        {
            string code = GetVerificationCode();
            if (code.Length == 6)
            {
                // Simular un pequeño delay para mejor UX
                Task.Delay(100).ContinueWith(t =>
                {
                    Dispatcher.Invoke(() => btnValidar_Click(null, null));
                });
            }
        }

        // Método para establecer un código programáticamente (útil para testing)
        public void SetVerificationCode(string code)
        {
            if (code.Length == 6 && code.All(char.IsDigit))
            {
                for (int i = 0; i < 6; i++)
                {
                    codeTextBoxes[i].Text = code[i].ToString();
                }
            }
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            LoginPage loginPage = new LoginPage();
            loginPage.ShowDialog();
        }

        private void btnValidar_Click(object sender, RoutedEventArgs e)
        {
            // Obtener el código de los 6 cuadros
            string codigoIngresado = GetVerificationCode();

            // Verificar que el código esté completo
            if (codigoIngresado.Length != 6)
            {
                MessageBox.Show(
                    "Por favor, ingresa el código de verificación completo (6 dígitos).",
                    "Código Incompleto",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // 1) ¿Expiró?
            if (DateTime.Now > RecoverySession.Expires)
            {
                MessageBox.Show(
                    "El código ha expirado. Solicita uno nuevo.",
                    "Expirado",
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
                this.Close();
                ForgotPasswordPage forgotPasswordPage = new ForgotPasswordPage();
                forgotPasswordPage.ShowDialog();
                return;
            }

            // 2) ¿Coincide?
            if (codigoIngresado.Trim() != RecoverySession.Code)
            {
                MessageBox.Show(
                    "Código incorrecto. Intenta de nuevo.",
                    "Inválido",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                // Limpiar los cuadros para permitir un nuevo intento
                ClearAllTextBoxes();
                return;
            }

            // 3) Correcto → abrir ResetPasswordPage
            this.Close();
            var reset = new ResetPasswordPage();
            reset.ShowDialog();
        }

        private void btnReenviarCodigo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Deshabilitar el botón temporalmente para evitar spam
                Button senderButton = sender as Button;
                senderButton.IsEnabled = false;
                senderButton.Content = "Enviando...";

                // Generar un nuevo código de 6 dígitos
                Random random = new Random();
                string newCode = random.Next(100000, 999999).ToString();

                // Actualizar la sesión de recuperación con el nuevo código
                RecoverySession.Code = newCode;
                RecoverySession.Expires = DateTime.Now.AddMinutes(15); // Válido por 15 minutos

                // Limpiar los cuadros para el nuevo código
                ClearAllTextBoxes();

                // Crear una instancia de ForgotPasswordPage para acceder al método de envío
                var forgotPasswordPage = new ForgotPasswordPage();

                // Enviar el nuevo código por correo usando la plantilla HTML
                bool enviado = forgotPasswordPage.EnviarCodigoRecuperacion(
                    ForgotPasswordPage.GlobalVariables.usuario,
                    ForgotPasswordPage.GlobalVariables.correoUsuario,
                    newCode
                );

                if (enviado)
                {
                    MessageBox.Show(
                        "Se ha enviado un nuevo código de verificación a tu correo electrónico.\n\n" +
                        "El código es válido por 15 minutos.",
                        "Código Reenviado",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    // Si hubo error al enviar, restaurar el código anterior
                    RecoverySession.Code = newCode; // Mantener el código generado para intentos locales
                    MessageBox.Show(
                        "No se pudo enviar el correo, pero se ha generado un nuevo código.\n\n" +
                        $"Código: {newCode}\n" +
                        "(Por favor, contacta al administrador si el problema persiste)",
                        "Error de Envío",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }

                // Rehabilitar el botón después de 30 segundos
                Task.Delay(30000).ContinueWith(t =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        senderButton.IsEnabled = true;
                        senderButton.Content = "Reenviar Código";
                    });
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ocurrió un error al generar el nuevo código. Por favor, intenta nuevamente.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                // Log del error para debugging
                System.Diagnostics.Debug.WriteLine($"Error en btnReenviarCodigo_Click: {ex.Message}");

                // Rehabilitar el botón en caso de error
                Button senderButton = sender as Button;
                senderButton.IsEnabled = true;
                senderButton.Content = "Reenviar Código";
            }
        }
    }
}