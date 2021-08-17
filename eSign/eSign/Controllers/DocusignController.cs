using DocuSign.eSign.Api;
using DocuSign.eSign.Client;
using DocuSign.eSign.Model;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using Document = DocuSign.eSign.Model.Document;
using eSign.Models;
using System.Linq;
using System.Data.Entity;

namespace DocusignDemo.Controllers
{
    public class DocusignController : Controller
    {
        MyCredential credential = new MyCredential();
        private string INTEGRATOR_KEY = "6ec07386-0da7-45e5-a45a-49ebbe450be9";


        public ActionResult SendDocumentforSign()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SendDocumentforSign(Recipient recipient, HttpPostedFileBase UploadDocument)
        {
            Recipient recipientModel = new Recipient();
            string directorypath = Server.MapPath("~/App_Data/" + "Files/");
            if (!Directory.Exists(directorypath))
            {
                Directory.CreateDirectory(directorypath);

            }

            byte[] data;
            using (Stream inputStream = UploadDocument.InputStream)
            {
                MemoryStream memoryStream = inputStream as MemoryStream;
                if (memoryStream == null)
                {
                    memoryStream = new MemoryStream();
                    inputStream.CopyTo(memoryStream);
                }
                data = memoryStream.ToArray();
            }

            var serverpath = directorypath + recipient.Title.Trim() + ".pdf";
            System.IO.File.WriteAllBytes(serverpath, data);
            docusign(serverpath, recipient.Title, recipient.Email);
            return View();
        }

        public string loginApi(string usr, string pwd)
        {
            // we set the api client in global config when we configured the client 
            ApiClient apiClient = Configuration.Default.ApiClient;
            string authHeader = "{\"Username\":\"" + usr + "\", \"Password\":\"" + pwd + "\", \"IntegratorKey\":\"" + INTEGRATOR_KEY + "\"}";
            Configuration.Default.AddDefaultHeader("X-DocuSign-Authentication", authHeader);

            // we will retrieve this from the login() results
            string accountId = null;

            // the authentication api uses the apiClient (and X-DocuSign-Authentication header) that are set in Configuration object
            AuthenticationApi authApi = new AuthenticationApi();
            LoginInformation loginInfo = authApi.Login();

            // find the default account for this user
            foreach (DocuSign.eSign.Model.LoginAccount loginAcct in loginInfo.LoginAccounts)
            {
                if (loginAcct.IsDefault == "true")
                {
                    accountId = loginAcct.AccountId;
                    break;
                }
            }
            if (accountId == null)
            { // if no default found set to first account
                accountId = loginInfo.LoginAccounts[0].AccountId;
            }
            return accountId;
        }

        public void docusign(string path, string recipientName, string recipientEmail)
        {

            ApiClient apiClient = new ApiClient("https://demo.docusign.net/restapi");
            Configuration.Default.ApiClient = apiClient;

            //Verify Account Details
            string accountId = loginApi(credential.UserName, credential.Password);

            // Read a file from disk to use as a document.
            byte[] fileBytes = System.IO.File.ReadAllBytes(path);

            EnvelopeDefinition envDef = new EnvelopeDefinition();
            envDef.EmailSubject = "Please sign this doc";

            // Add a document to the envelope
            Document doc = new Document();
            doc.DocumentBase64 = System.Convert.ToBase64String(fileBytes);
            doc.Name = Path.GetFileName(path);
            doc.DocumentId = "1";

            envDef.Documents = new List<Document>();
            envDef.Documents.Add(doc);

            // Add a recipient to sign the documeent
            DocuSign.eSign.Model.Signer signer = new DocuSign.eSign.Model.Signer();
            signer.Email = recipientEmail;
            signer.Name = recipientName;
            signer.RecipientId = "1";

            envDef.Recipients = new DocuSign.eSign.Model.Recipients();
            envDef.Recipients.Signers = new List<DocuSign.eSign.Model.Signer>();
            envDef.Recipients.Signers.Add(signer);

            //set envelope status to "sent" to immediately send the signature request
            envDef.Status = "sent";

            // |EnvelopesApi| contains methods related to creating and sending Envelopes (aka signature requests)
            EnvelopesApi envelopesApi = new EnvelopesApi();
            EnvelopeSummary envelopeSummary = envelopesApi.CreateEnvelope(accountId, envDef);

            // print the JSON response
            var result = JsonConvert.SerializeObject(envelopeSummary);

            Recipient recipient = new Recipient();
            recipient.Description = "envDef.EmailSubject";
            recipient.Email = recipientEmail;
            recipient.Title = recipientName;
            recipient.Status = envelopeSummary.Status;
            recipient.Documents = fileBytes;
            recipient.CreationDate = System.Convert.ToDateTime(envelopeSummary.StatusDateTime);
            recipient.EnvelopeID = envelopeSummary.EnvelopeId;

            ESignDocumentEntities eSignDocumentEntities = new ESignDocumentEntities();
            eSignDocumentEntities.Recipients.Add(recipient);
            eSignDocumentEntities.SaveChanges();
        }

        public ActionResult getEnvelopeInformation()
        {
            ApiClient apiClient = new ApiClient("https://demo.docusign.net/restapi");
            Configuration.Default.ApiClient = apiClient;

            // provide a valid envelope ID from your account.  
            string envelopeId = "6ec07386-0da7-45e5-a45a-49ebbe450be9"; //Enter Stored Envelope Id
            MyCredential myCredential = new MyCredential();

            string accountId = loginApi(myCredential.UserName, myCredential.Password);


            // |EnvelopesApi| contains methods related to creating and sending Envelopes including status calls
            EnvelopesApi envelopesApi = new EnvelopesApi();
            Envelope envInfo = envelopesApi.GetEnvelope(accountId, envelopeId);
            if (envInfo.Status == "completed")
            {
                ESignDocumentEntities eSignDocumentEntities = new ESignDocumentEntities();
                var recipient = eSignDocumentEntities.Recipients.Where(a => a.EnvelopeID == envelopeId).FirstOrDefault();
                recipient.Status = "completed";
                recipient.UpdateOn = System.DateTime.Now;
                eSignDocumentEntities.Entry(recipient).State = EntityState.Modified;
                eSignDocumentEntities.SaveChanges();
            }
            return View();
        } // end requestSignatu

        List<Recipient> recipientsDocs = new List<Recipient>();
        public ActionResult ListDocuments()
        {
            ApiClient apiClient = new ApiClient("https://demo.docusign.net/restapi");
            Configuration.Default.ApiClient = apiClient;
            MyCredential myCredential = new MyCredential();

            // call the Login() API which sets the user's baseUrl and returns their accountId
            string accountId = loginApi(myCredential.UserName, myCredential.Password);

            ESignDocumentEntities eSignDocumentEntities = new ESignDocumentEntities();
            var recipients = eSignDocumentEntities.Recipients.ToList();
            string serverDirectory = Server.MapPath("~/Uploadfiles/");
            if (!Directory.Exists(serverDirectory))
            {
                Directory.CreateDirectory(serverDirectory);
            }

            foreach (var recipient in recipients)
            {
                string recipientDirectory = Server.MapPath("~/Uploadfiles/" + recipient.EnvelopeID);
                if (!Directory.Exists(recipientDirectory))
                {
                    Directory.CreateDirectory(recipientDirectory);
                }

                EnvelopeDocumentsResult documentList = ListEnvelopeDocuments(accountId, recipient.EnvelopeID);

                int i = 0;
                string SignedPDF = string.Empty;
                EnvelopesApi envelopesApi = new EnvelopesApi();
                foreach (var document in documentList.EnvelopeDocuments)
                {
                    string signingStatus = recipient.Status == "completed" ? "Signed" : "Yet to Sign";
                    MemoryStream docStream = (MemoryStream)envelopesApi.GetDocument(accountId, recipient.EnvelopeID, documentList.EnvelopeDocuments[i].DocumentId);
                    string documentName = document.Name != "Summary" ? document.Name : "Summary";
                    SignedPDF = Server.MapPath("~/Uploadfiles/" + recipient.EnvelopeID + "/" + recipient.EnvelopeID + "_" + documentName + ".pdf");
                    using (var fileStream = System.IO.File.Create(SignedPDF))
                    {
                        docStream.Seek(0, SeekOrigin.Begin);
                        docStream.CopyTo(fileStream);
                    }

                    recipientsDocs.Add(new Recipient { EnvelopeID = recipient.EnvelopeID, Title = recipient.Title, Email = recipient.Email, Status = signingStatus, documentURL = SignedPDF, CreationDate = recipient.CreationDate, UpdateOn = recipient.UpdateOn });

                    i++;
                }
            }

            return View(recipientsDocs);
        }

        public EnvelopeDocumentsResult ListEnvelopeDocuments(string accountId, string envelopeId)
        {
            EnvelopesApi envelopesApi = new EnvelopesApi();
            EnvelopeDocumentsResult docsList = envelopesApi.ListDocuments(accountId, envelopeId);
            return docsList;
        }

        public FileResult Download(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            string contentType = "application/pdf";
            return File(filePath, contentType, fileName);
        }
    }

    public class MyCredential
    {
        public string UserName { get; set; } = "steniojosephs@gmail.com";
        public string Password { get; set; } = "Dev$jme90";
    }
}