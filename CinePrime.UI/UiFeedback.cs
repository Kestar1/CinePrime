using System;
using System.Windows.Forms;

namespace CinePrime.UI
{
    public static class UiFeedback
    {
        public static void ShowResult(Form owner, (bool Success, string Message) result, string successTitle = "Succes", string errorTitle = "Eroare")
        {
            if (result.Success)
            {
                ToastNotification.Show(owner, result.Message, ToastType.Success);
                MessageBox.Show(owner, result.Message, successTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                ToastNotification.Show(owner, result.Message, ToastType.Error);
                MessageBox.Show(owner, result.Message, errorTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public static void ShowInfo(Form owner, string message, string title = "Info")
        {
            ToastNotification.Show(owner, message, ToastType.Info);
            MessageBox.Show(owner, message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void ShowWarning(Form owner, string message, string title = "Atentie")
        {
            ToastNotification.Show(owner, message, ToastType.Warning);
            MessageBox.Show(owner, message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void ShowError(Form owner, string message, string title = "Eroare")
        {
            ToastNotification.Show(owner, message, ToastType.Error);
            MessageBox.Show(owner, message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void ShowException(Form owner, Exception ex, string userMessage)
        {
            var message = string.IsNullOrWhiteSpace(ex?.Message)
                ? userMessage
                : $"{userMessage}{Environment.NewLine}{Environment.NewLine}Detalii: {ex.Message}";
            ShowError(owner, message, "Exceptie");
        }
    }
}
