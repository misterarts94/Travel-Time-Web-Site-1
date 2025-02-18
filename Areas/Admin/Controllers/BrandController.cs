using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TopSpeed.Domain.Models;
using Top_Speed.Infrastructure.Common;
using TopSpeed.Application.ApplicationConstants;
using TopSpeed.Application.Contracts.Presistence;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using TopSpeed.Domain.ApplicationEnums;
using Microsoft.AspNetCore.Authorization;

namespace TopSpeed.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = $"{CustomRole.MasterAdmin},{CustomRole.Admin}")]
    //[Authorize(Roles = CustomRole.MasterAdmin + "," + CustomRole.Admin)]
    public class BrandController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<BrandController> _logger;

        public BrandController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment, ILogger<BrandController> logger)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

       

        [HttpGet]

        public async Task<IActionResult> Index()
        {
            try
            {
                List<Brand> brands = await _unitOfWork.Brand.GatAllAsync();
                _logger.LogInformation("Brand List Fetched from Database Successfully");
                return View(brands);
            }
            catch (Exception ex) 
            {
                _logger.LogError("Something Went Wrong");
                return View();
            
            }
        }


        [AllowAnonymous] // Makes this action accessible to everyone
        public IActionResult PublicAction()
        {
            return View();
        }


        [Authorize(Roles = CustomRole.MasterAdmin)] // Only MasterAdmin can access
        public IActionResult MasterAdminOnly()
        {
            return View();
        }

        [HttpGet]

        public IActionResult Create()
        {
            

            return View();
        }

        [HttpPost]

        public async Task<IActionResult> Create(Brand brand)
        {
            string WebRootPath = _webHostEnvironment.WebRootPath;

            var file = HttpContext.Request.Form.Files;

            if (file.Count > 0)
            {
                string newFileName = Guid.NewGuid().ToString();

                var upload = Path.Combine(WebRootPath, @"images\brand\");

                var extension = Path.GetExtension(file[0].FileName);

                using (var fileStream = new FileStream(Path.Combine(upload, newFileName + extension), FileMode.Create))
                {
                    file[0].CopyTo(fileStream);
                }

                brand.BrandLogo = @"\images\brand\" + newFileName + extension;
            }

            if (ModelState.IsValid)
            {
                await _unitOfWork.Brand.Create(brand);
                await _unitOfWork.SaveAsync();

                TempData["Success"] = CommonMessage.RecordCreated;

                return RedirectToAction(nameof(Index));
            }

            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            Brand brand = await _unitOfWork.Brand.GatByIdAsync(id);

            return View(brand);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            Brand brand = await _unitOfWork.Brand.GatByIdAsync(id);

            return View(brand);
        }

        [HttpPost]
        //edit post

        public async Task<IActionResult> Edit(Brand brand)
        {
            string WebRootPath = _webHostEnvironment.WebRootPath;

            var file = HttpContext.Request.Form.Files;

            if (file.Count > 0)
            {
                string newFileName = Guid.NewGuid().ToString();

                var upload = Path.Combine(WebRootPath, @"images\brand\");

                var extension = Path.GetExtension(file[0].FileName);

                //delete old images

                var oldFromDb = await _unitOfWork.Brand.GatByIdAsync(brand.Id);

                if (oldFromDb.BrandLogo != null)
                {
                    string oldImagePath = Path.Combine(WebRootPath, oldFromDb.BrandLogo.Trim('\\'));

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                using (var fileStream = new FileStream(Path.Combine(upload, newFileName + extension), FileMode.Create))
                {
                    file[0].CopyTo(fileStream);
                }

                brand.BrandLogo = @"\images\brand\" + newFileName + extension;
            }
            if (ModelState.IsValid)
            {    //Edit post Path bug fix
                await _unitOfWork.Brand.Update(brand);
                await _unitOfWork.SaveAsync();

                TempData["Success"] = CommonMessage.RecordUpdate;

                return RedirectToAction(nameof(Index));
            }

            return View();

        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            Brand brand = await _unitOfWork.Brand.GatByIdAsync(id);
            return View(brand);
        }

        [HttpPost]
        // delete post
        public async Task<IActionResult> Delete(Brand brand)
        {
            string WebRootPath = _webHostEnvironment.WebRootPath;

            if (!string.IsNullOrEmpty(WebRootPath))
            {
                var oldFromDb = await _unitOfWork.Brand.GatByIdAsync(brand.Id);

                if (oldFromDb.BrandLogo != null)
                {
                    string oldImagePath = Path.Combine(WebRootPath, oldFromDb.BrandLogo.Trim('\\'));

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }
            }

            await _unitOfWork.Brand.Delete(brand);
            await _unitOfWork.SaveAsync();

            TempData["Success"] = CommonMessage.RecordDelete;

            return RedirectToAction(nameof(Index));
        }






    }
}
