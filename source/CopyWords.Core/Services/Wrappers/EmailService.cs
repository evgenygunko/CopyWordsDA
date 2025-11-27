namespace CopyWords.Core.Services.Wrappers
{
    public interface IEmailService
    {
        Task ComposeAsync(string subject, string body, List<string>? recipients = null);
    }

    public class EmailService : IEmailService
    {
        public async Task ComposeAsync(string subject, string body, List<string>? recipients = null)
        {
            // Build Gmail compose URL with parameters
            var gmailUrl = "https://mail.google.com/mail/?view=cm&fs=1";

            if (!string.IsNullOrEmpty(subject))
            {
                gmailUrl += $"&su={Uri.EscapeDataString(subject)}";
            }

            if (!string.IsNullOrEmpty(body))
            {
                gmailUrl += $"&body={Uri.EscapeDataString(body)}";
            }

            if (recipients != null && recipients.Any())
            {
                var recipientList = string.Join(",", recipients);
                gmailUrl += $"&to={Uri.EscapeDataString(recipientList)}";
            }

            try
            {
                await Task.Run(() =>
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = gmailUrl,
                        UseShellExecute = true
                    });
                });
            }
            catch (System.ComponentModel.Win32Exception noBrowser)
            {
                if (noBrowser.ErrorCode == -2147467259)
                {
                    throw new InvalidOperationException("No default browser is configured on this system.", noBrowser);
                }
                throw new InvalidOperationException("Failed to open browser.", noBrowser);
            }
            catch (Exception other)
            {
                throw new InvalidOperationException("Failed to compose email in browser.", other);
            }
        }
    }
}