using EmployeeManagement.Models;
using EmployeeManagement.Security;
using EmployeeManagement.ViewModels;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagement.Controllers
{
    [Authorize]
     public class HomeController : Controller
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IHostingEnvironment hostingEnvironment;
        private readonly ILogger logger;
        private readonly IDataProtector protector;
        public HomeController(IEmployeeRepository employeeRepository,
                              IHostingEnvironment hostingEnvironment,
                              ILogger<HomeController> logger,
                              IDataProtectionProvider dataProtectionProvider,
                              DataProtectionPurposeStrings dataProtectionPurposeStrings)
        {   
            _employeeRepository = employeeRepository;
            this.hostingEnvironment = hostingEnvironment;
            this.logger = logger;
            protector = dataProtectionProvider.CreateProtector(dataProtectionPurposeStrings.EmployeeIdRouteValue);
        }

        [AllowAnonymous]
        public ViewResult Index()
        {
            var model = _employeeRepository.GetAllEmployees()
                        .Select(e =>
                        {
                            e.EncryptedId = protector.Protect(e.Id.ToString());
                            return e;
                        });

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //var message = new MimeMessage();

            //message.From.Add(new MailboxAddress("Test Project", "skbk1996@gmail.com"));
            //message.To.Add(new MailboxAddress("Bittu", "skbk404@gmail.com"));
            //message.Subject = "Welcome to MimeKit";

            //var bodyBuilder = new BodyBuilder();
            //bodyBuilder.HtmlBody = "<a href='google.com'>Google</a>";

            //bodyBuilder.Attachments.Add("C:/Users/SAURAV KUMAR/Downloads/pie.jpg");


            //message.Body = bodyBuilder.ToMessageBody();

            //using (var client = new SmtpClient())
            //{
            //    client.Connect("smtp.gmail.com", 587, false);
            //    client.Authenticate("skbk1996@gmail.com", "9031141170s");
            //    client.Send(message);
            //    client.Disconnect(true);
            //}



            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



            return View(model);
        }

        [AllowAnonymous]
        public ViewResult Details(string id)
        {
            //throw new Exception("Errors in Details View");

            logger.LogTrace("Trace Log");
            logger.LogDebug("Debug Log");
            logger.LogInformation("Information Log");
            logger.LogWarning("Warning Log");
            logger.LogError("Error Log");
            logger.LogCritical("Critical Log");

            int employeeId = Convert.ToInt32(protector.Unprotect(id));

            Employee employee = _employeeRepository.GetEmployee(employeeId);

            if(employee == null)
            {
                Response.StatusCode = 404;
                return View("EmployeeNotFound", employeeId);
            }

            HomeDetailsViewModel homeDetailsViewModel = new HomeDetailsViewModel()
            {
                Employee = employee,
                PageTitle = "Employee Details"
            };
            return View(homeDetailsViewModel);
        }

        public ViewResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(EmployeeCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                string uniqueFileName = ProcessUploadFile(model);
                //if (model.Photo != null && model.Photo.Count()>0)
                //{
                //    foreach (IFormFile photo in model.Photo)
                //    {
                //        string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images");
                //        uniqueFileName = Guid.NewGuid().ToString() + "_" + photo.FileName;
                //        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                //        photo.CopyTo(new FileStream(filePath, FileMode.Create));
                //    }
                //}

               

                Employee newEmployee = new Employee
                {
                    Name = model.Name,
                    Email = model.Email,
                    Department = model.Department,
                    PhotoPath = uniqueFileName
                };
                _employeeRepository.Add(newEmployee);
                return RedirectToAction("Details", new { id = newEmployee.Id });
            }
            return View();
        }
        public ViewResult Edit(int id)
        {
            Employee employee = _employeeRepository.GetEmployee(id);
            EmployeeEditViewModel employeeEditViewModel = new EmployeeEditViewModel 
            {
                Id = employee.Id,
                Name = employee.Name,
                Email = employee.Email,
                Department = employee.Department,
                ExistingPhotoPath = employee.PhotoPath
            };
            return View(employeeEditViewModel);
        }

        [HttpPost]
        public IActionResult Edit(EmployeeEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                Employee employee = _employeeRepository.GetEmployee(model.Id);

                employee.Name = model.Name;
                employee.Email = model.Email;
                employee.Department = model.Department;

                if (model.Photo != null)
                {
                    if(model.ExistingPhotoPath != null)
                    {
                        string filePath = Path.Combine(hostingEnvironment.WebRootPath,"images",model.ExistingPhotoPath);
                        System.IO.File.Delete(filePath);
                    }
                   employee.PhotoPath = ProcessUploadFile(model);
                }
                _employeeRepository.Update(employee);
                return RedirectToAction("Index");
            }
            return View();
        }

        private string ProcessUploadFile(EmployeeCreateViewModel model)
        {
            string uniqueFileName = null;

            if (model.Photo != null)
            {
                string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Photo.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    model.Photo.CopyTo(fileStream);
                }
                
            }

            return uniqueFileName;
        }
    }
}
