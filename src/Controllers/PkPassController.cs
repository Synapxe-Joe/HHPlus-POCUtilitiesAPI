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
            PassGeneratorRequest passGeneratorRequest = new PassGeneratorRequest();
            passGeneratorRequest.PassTypeIdentifier = "Apple Push Services: com.hpb.HealthHub.sit.rebuild";
            passGeneratorRequest.TeamIdentifier = "FJZERSG4G2"; // Replace with your actual team identifier
            passGeneratorRequest.SerialNumber = Guid.NewGuid().ToString();
            passGeneratorRequest.Description = "Emergency Contact";
            passGeneratorRequest.OrganizationName = "Synapxe";

            passGeneratorRequest.Style = PassStyle.Generic;


            // Load certificate
            string certificatePath = "Certificates/Certificates.p12" // Update as needed
            , certificatePassword = "Syn@pxe2025"    // Update as needed
            , appleWWDRCAPath = "Certificates/AppleWWDRCAG3.cer"; // Update as needed   


            X509Certificate2 certificate = new X509Certificate2(certificatePath, certificatePassword, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet);
            X509Certificate2 appleWWDRCA = new X509Certificate2(appleWWDRCAPath);

            passGeneratorRequest.PassbookCertificate = certificate;
            passGeneratorRequest.AppleWWDRCACertificate = appleWWDRCA;

            passGeneratorRequest.AddPrimaryField(new StandardField("name", "name", emergencyContact.Name));
            passGeneratorRequest.AddPrimaryField(new StandardField("contact", "contact", emergencyContact.Contact));

            passGeneratorRequest.Images.Add(PassbookImage.Icon, System.IO.File.ReadAllBytes("Resources/icon.png")); 
            passGeneratorRequest.Images.Add(PassbookImage.Icon2X, System.IO.File.ReadAllBytes("Resources/icon@2x.png")); 

            PassGenerator passGenerator = new PassGenerator();
            using var pkPassStream = new MemoryStream();

            try
            {
                byte[] generatedPass = passGenerator.Generate(passGeneratorRequest);

                string contentType = string.Empty, fileName = string.Empty;
                switch (emergencyContact.Mode)
                {
                    case "wallet":                        
                        contentType = "application/vnd.apple.pkpass";
                        fileName = "EmergencyContact.pkpass";
                        break;
                    default:
                        contentType = "application/zip";
                        fileName = "EmergencyContact.zip";
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
