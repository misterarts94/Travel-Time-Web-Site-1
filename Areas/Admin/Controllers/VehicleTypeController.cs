using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TopSpeed.Domain.Models;
using Top_Speed.Infrastructure.Common;
using TopSpeed.Application.ApplicationConstants;
using TopSpeed.Application.Contracts.Presistence;
using Microsoft.AspNetCore.Authorization;


namespace TopSpeed.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize (Roles = $"{CustomRole.MasterAdmin},{CustomRole.Admin}")]
    //[Authorize(Roles = CustomRole.MasterAdmin + "," + CustomRole.Admin)]
    public class VehicleTypeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public VehicleTypeController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]

        public async Task<IActionResult> Index()
        {
            List<VehicleType> vehicleType = await _unitOfWork.VehicleType.GatAllAsync();

            return View(vehicleType);
        } 

        [HttpGet]

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]

        public async Task<IActionResult> Create(VehicleType vehicleType)
        {
           

            if (ModelState.IsValid)
            {
                await _unitOfWork.VehicleType.Create(vehicleType );
                await _unitOfWork.SaveAsync();

                TempData["Success"] = CommonMessage.RecordCreated;

                return RedirectToAction(nameof(Index));
            }

            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            VehicleType vehicleType = await _unitOfWork.VehicleType.GatByIdAsync(id);

            return View(vehicleType);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            VehicleType vehicleType = await _unitOfWork.VehicleType.GatByIdAsync(id);

            return View(vehicleType);
        }

        [HttpPost]
        //edit post

        public async Task<IActionResult> Edit(VehicleType vehicleType)
        {
            
            if (ModelState.IsValid)
            {    //Edit post Path bug fix
                await _unitOfWork.VehicleType.Update(vehicleType);
                await _unitOfWork.SaveAsync();

                TempData["Success"] = CommonMessage.RecordUpdate;

                return RedirectToAction(nameof(Index));
            }

            return View();

        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            VehicleType vehicleType = await _unitOfWork.VehicleType.GatByIdAsync(id);

            return View(vehicleType);
        }

        [HttpPost]
        // delete post
        public async Task<IActionResult> Delete(VehicleType vehicleType)
        {
           

            await _unitOfWork.VehicleType.Delete(vehicleType );
            await _unitOfWork.SaveAsync();

            TempData["Success"] = CommonMessage.RecordDelete;

            return RedirectToAction(nameof(Index));
        }






    }
}
