using Microsoft.AspNetCore.Mvc;
using Passbook.Generator;
using Passbook.Generator.Fields;
using POC.Utilities.API.Models;
using System.Security.Cryptography.X509Certificates;
using System.IO;

namespace POC.Utilities.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PkPassController : ControllerBase
    {
        [HttpGet]
        [HttpPost]

        public IActionResult Index([FromBody][FromQuery] EmergencyContact emergencyContact)
        {
            string serialNumber = Guid.NewGuid().ToString();

            PassGeneratorRequest passGeneratorRequest = new PassGeneratorRequest();            
            passGeneratorRequest.PassTypeIdentifier = "Pass Type ID: pass.Healthhub";
            passGeneratorRequest.TeamIdentifier = "FJZERSG4G2"; // Replace with your actual team identifier
            passGeneratorRequest.SerialNumber = serialNumber;
            passGeneratorRequest.Description = "Emergency Contact";
            passGeneratorRequest.OrganizationName = "Synapxe";
            passGeneratorRequest.LogoText = "Emergency Contact";
            
            passGeneratorRequest.Style = PassStyle.Generic;
            passGeneratorRequest.BackgroundColor = "#FFFFFF";
            passGeneratorRequest.LabelColor = "#000000";
            passGeneratorRequest.ForegroundColor = "#000000";

            // Load certificate
            string certificatePath = "Certificates/Certificates.p12" // Update as needed               
            , certificatePassword = "Syn@pxe2025"    // Update as needed
            , passCertficatePath = "Certificates/pass.cer" // Update as needed
            , appleWWDRCAPath = "Certificates/AppleWWDRCAG4.cer"; // Update as needed   


            X509Certificate2 certificate = new X509Certificate2(certificatePath, certificatePassword, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet);            
            X509Certificate2 appleWWDRCA = new X509Certificate2(appleWWDRCAPath);

            passGeneratorRequest.PassbookCertificate = certificate;            
            passGeneratorRequest.AppleWWDRCACertificate = appleWWDRCA;

            string contactNo = emergencyContact.Contact.StartsWith("+") ? emergencyContact.Contact : $"+65{emergencyContact.Contact}";
            passGeneratorRequest.AddPrimaryField(new StandardField("name", "name", emergencyContact.Name));
            passGeneratorRequest.AddPrimaryField(new StandardField("contact", "contact", contactNo,
               "<a href='tel:" + contactNo + "'>" + contactNo + "</a>", DataDetectorTypes.PKDataDetectorTypePhoneNumber));
                

            passGeneratorRequest.Images.Add(PassbookImage.Icon, System.IO.File.ReadAllBytes("Resources/icon.png")); 
            passGeneratorRequest.Images.Add(PassbookImage.Icon2X, System.IO.File.ReadAllBytes("Resources/icon@2x.png")); 

            PassGenerator passGenerator = new PassGenerator();            

            try
            {
                byte[] generatedPass = passGenerator.Generate(passGeneratorRequest);

                string contentType = string.Empty, fileName = string.Empty;
                switch (emergencyContact.Mode)
                {
                    case "wallet":                        
                        contentType = "application/vnd.apple.pkpass";
                        fileName = $"EmergencyContact_{serialNumber}.pkpass";
                        break;
                    default:
                        contentType = "application/zip";
                        fileName = $"EmergencyContact_{serialNumber}.zip";
                        break;
                }
                return File(generatedPass, contentType, fileName);
            }
            catch (Exception ex)
            {
                return BadRequest("Failed to generate pass: " + ex.Message);
            }
        }
    }
}
