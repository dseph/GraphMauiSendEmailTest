 using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

//using Windows.Data.Xml.Dom;
 using Microsoft.Graph;
using Microsoft.Graph.Models;
 using Azure.Identity;
using AttachmentUpload = Microsoft.Graph.Me.Messages.Item.Attachments.CreateUploadSession;

namespace GraphMauiSendEmailTest
{
    public partial class MainPage : ContentPage
    {
        private string _ClientAppId = string.Empty;
        private string _TenantId = string.Empty;
        private string _ClientSecret = string.Empty;
        private string _FromSmtp = string.Empty;
        private string _ToSmtp = string.Empty;
        private string _Subject = string.Empty;
        private string _Body = string.Empty;
        private string _AttatchmentFilePath = string.Empty;

        public MainPage()
        {
            InitializeComponent();
        }

        private void On_SendEmail_Clicked(object sender, EventArgs e)
        {

            _ClientAppId = AppIdEntry.Text.Trim();  
            _TenantId = TenantIdEntry.Text.Trim();  
            _ClientSecret = ClientSecretEntry.Text.Trim();

    
            _ToSmtp = ToAddress.Text.Trim();
            _Subject = Subject.Text.Trim();
            _Body = Body.Text;
            _AttatchmentFilePath = AttatchmentFilePath.Text;

            SendEmail();

   
        }

        private async void SendEmail()
        {
            var tenantId = _TenantId;
            var clientId = _ClientAppId;
            var clientSecret = _ClientSecret;

            var clientSecretCredential = new ClientSecretCredential(tenantId, clientId, clientSecret);
            var graphClient = new GraphServiceClient(clientSecretCredential);

            // Create a draft message
            var draftMessage = new Message
            {
                Subject = _Body,
                Body = new ItemBody
                {
                    ContentType = BodyType.Text,
                    Content = _Body
                },
                ToRecipients = new List<Recipient>
                {
                    new Recipient
                    {
                        EmailAddress = new EmailAddress
                        {
                            Address = _ToSmtp
                        }
                    }
                },
                //InternetMessageHeaders = new List<InternetMessageHeader>
                //{
                //    new InternetMessageHeader
                //    {
                //        Name = "x-custom-header-group-name",
                //        Value = "Washington",
                //    },
                //    new InternetMessageHeader
                //    {
                //        Name = "x-custom-header-group-id",
                //        Value = "WA001",
                //    },
                //},
            };

            //var savedDraft = await graphClient.Me.Messages.Request().AddAsync(draftMessage);
           // var savedDraft = await graphClient.Me.Messages.Request().AddAsync(draftMessage);
           // var savedDraft = await graphClient.Me.Messages.GetAsync.GetAsync().AddAsync(draftMessage);
            var savedDraft = await graphClient.Me.Messages.PostAsync(draftMessage);


            // var result = await graphClient.Me.MailFolders["{mailFolder-id}"].Messages.PostAsync(requestBody);

            string sName = Path.GetFileName(_AttatchmentFilePath);
            // Create an upload session
            var attachmentItem = new AttachmentItem
            {
                AttachmentType = AttachmentType.File,
                Name = sName,
                Size = new FileInfo(_AttatchmentFilePath).Length
            };

            //var uploadSession = await graphClient.Me.Messages[savedDraft?.Id].Attachments
            //    .CreateUploadSession(attachmentItem)
            //    .Request()
            //    .PostAsync();

            var uploadSessionRequestBody = new AttachmentUpload.CreateUploadSessionPostRequestBody
            {
                AttachmentItem = attachmentItem,
            };

            var uploadSession = await graphClient.Me
                .Messages[savedDraft?.Id]
                .Attachments
                .CreateUploadSession
                .PostAsync(uploadSessionRequestBody);


            using var fileStream = File.OpenRead(_AttatchmentFilePath);
            var maxSliceSize = 320 * 1024; // 320 KB
            var fileUploadTask = new LargeFileUploadTask<AttachmentItem>(uploadSession, fileStream, maxSliceSize, graphClient.RequestAdapter);

            IProgress<long> progress = new Progress<long>(prog => {
                Console.WriteLine($"Uploaded {prog} bytes of {fileStream.Length} bytes");
            });

            var uploadResult = await fileUploadTask.UploadAsync(progress);

            //string s = uploadResult.ItemResponse.ContentId;

            if (uploadResult.UploadSucceeded)
            {
                //Console.WriteLine($"Upload complete, item ID: {uploadResult.ItemResponse.}");
                Console.WriteLine("Upload completed");
            }
            else
            {
                Console.WriteLine("Upload failed");
            }

            // Send the email
            //await graphClient.Me.Messages[savedDraft?.Id]
            //    .Send()
            //    .Request()
            //    .PostAsync();

            await graphClient.Me.Messages[savedDraft?.Id].Send.PostAsync();
    
        }

 
 
    }

}
