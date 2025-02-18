using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TopSpeed.Domain.Models;
using Top_Speed.Infrastructure.Common;
using TopSpeed.Application.ApplicationConstants;
using TopSpeed.Application.Contracts.Presistence;
using TopSpeed.Domain.ApplicationEnums;
using Microsoft.AspNetCore.Mvc.Rendering;
using TopSpeed.Domain.ViewModel;
using Top_Speed.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using TopSpeed.Application.Service.Interface;

namespace TopSpeed.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = $"{CustomRole.MasterAdmin},{CustomRole.Admin}")]
    //[Authorize(Roles = CustomRole.MasterAdmin + "," + CustomRole.Admin)]
    public class PostController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IPostRepository _postRepository;
        private readonly IUserNameService _userName;

        // Remove IPostRepository from the constructor
        public PostController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment, IPostRepository postRepository, IUserNameService userName)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
            _postRepository = postRepository;
            _userName = userName;
        }

        // Update methods to use _unitOfWork.Post instead of _postRepository
        public async Task<IActionResult> Index()
        {
            List<Post> posts = await _unitOfWork.Post.GetAllPost();
            return View(posts);
        }

        [HttpGet]

        public IActionResult Create()
        {
            IEnumerable<SelectListItem> brandList = _unitOfWork.Brand.Query().Select(x => new SelectListItem
            {
                Text = x.Name.ToUpper(),
                Value = x.Id.ToString()
            });
            IEnumerable<SelectListItem> vehicleTypeList = _unitOfWork.VehicleType.Query().Select(x => new SelectListItem
            {
                Text = x.Name.ToUpper(),
                Value = x.Id.ToString()
            });
            IEnumerable<SelectListItem> engineAndFuelType = Enum.GetValues(typeof(EngineAndFuelType)).Cast<EngineAndFuelType>().Select(x => new SelectListItem
            {
                Text = x.ToString().ToUpper(),
                Value = ((int)x).ToString()
            });
            IEnumerable<SelectListItem> transmission = Enum.GetValues(typeof(Transmission)).Cast<Transmission>().Select(x => new SelectListItem
            {
                Text = x.ToString().ToUpper(),
                Value = ((int)x).ToString()
            });

            PostVM postVM = new PostVM
            {
                Post = new Post(),
                BrandList = brandList,
                VehicleTypeList = vehicleTypeList,
                EngineAndFuelTypeList = engineAndFuelType,
                TransmissionList = transmission


            };

            return View(postVM);
        }

        [HttpPost]

        public async Task<IActionResult> Create(PostVM postVM)
        {
            string WebRootPath = _webHostEnvironment.WebRootPath;

            var file = HttpContext.Request.Form.Files;

            if (file.Count > 0)
            {
                string newFileName = Guid.NewGuid().ToString();

                var upload = Path.Combine(WebRootPath, @"images\post\");

                var extension = Path.GetExtension(file[0].FileName);

                using (var fileStream = new FileStream(Path.Combine(upload, newFileName + extension), FileMode.Create))
                {
                    file[0].CopyTo(fileStream);
                }

                postVM.Post.VehicleImage = @"\images\post\" + newFileName + extension;
            }

            if (ModelState.IsValid)
            {
                await _unitOfWork.Post.Create(postVM.Post);
                await _unitOfWork.SaveAsync();

                TempData["Success"] = CommonMessage.RecordCreated;

                return RedirectToAction(nameof(Index));
            }

            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            Post post = await _unitOfWork.Post.GetPostById(id);

            post.CreatedBy = await _userName.GetUserName(post.CreatedBy);

            post.ModifiedBy = await _userName.GetUserName(post.ModifiedBy);

            return View(post);
        }
        
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            Post post = await _unitOfWork.Post.GetPostById(id);

            // Populate dropdown lists
            IEnumerable<SelectListItem> brandList = _unitOfWork.Brand.Query().Select(x => new SelectListItem
            {
                Text = x.Name.ToUpper(),
                Value = x.Id.ToString()
            });

            IEnumerable<SelectListItem> vehicleTypeList = _unitOfWork.VehicleType.Query().Select(x => new SelectListItem
            {
                Text = x.Name.ToUpper(),
                Value = x.Id.ToString()
            });

            IEnumerable<SelectListItem> engineAndFuelType = Enum.GetValues(typeof(EngineAndFuelType))
                .Cast<EngineAndFuelType>()
                .Select(x => new SelectListItem
                {
                    Text = x.ToString().ToUpper(),
                    Value = ((int)x).ToString()
                });

            IEnumerable<SelectListItem> transmission = Enum.GetValues(typeof(Transmission))
                .Cast<Transmission>()
                .Select(x => new SelectListItem
                {
                    Text = x.ToString().ToUpper(),
                    Value = ((int)x).ToString()
                });

            // Create PostVM with dropdown data
            PostVM postVM = new PostVM
            {
                Post = post,
                BrandList = brandList,
                VehicleTypeList = vehicleTypeList,
                EngineAndFuelTypeList = engineAndFuelType,
                TransmissionList = transmission
            };

            return View(postVM); // ✅ Pass the full PostVM
        }

        [HttpPost]
        //edit post

        public async Task<IActionResult> Edit(PostVM postVM)
        {
            string WebRootPath = _webHostEnvironment.WebRootPath;

            var file = HttpContext.Request.Form.Files;

            if (file.Count > 0)
            {
                string newFileName = Guid.NewGuid().ToString();

                var upload = Path.Combine(WebRootPath, @"images\post\");

                var extension = Path.GetExtension(file[0].FileName);

                //delete old images

                var oldFromDb = await _unitOfWork.Post.GetPostById(postVM.Post.Id);

                if (oldFromDb.VehicleImage != null)
                {
                    string oldImagePath = Path.Combine(WebRootPath, oldFromDb.VehicleImage.Trim('\\'));

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                using (var fileStream = new FileStream(Path.Combine(upload, newFileName + extension), FileMode.Create))
                {
                    file[0].CopyTo(fileStream);
                }

                postVM.Post.VehicleImage = @"\images\post\" + newFileName + extension;
            }
            if (ModelState.IsValid)
            {    //Edit post Path bug fix
                await _unitOfWork.Post.Update(postVM.Post);
                await _unitOfWork.SaveAsync();

                TempData["Success"] = CommonMessage.RecordUpdate;

                return RedirectToAction(nameof(Index));
            }

            return View();

        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            
            Post post = await _unitOfWork.Post.GetPostById(id);

            // Populate dropdown lists
            IEnumerable<SelectListItem> brandList = _unitOfWork.Brand.Query().Select(x => new SelectListItem
            {
                Text = x.Name.ToUpper(),
                Value = x.Id.ToString()
            });

            IEnumerable<SelectListItem> vehicleTypeList = _unitOfWork.VehicleType.Query().Select(x => new SelectListItem
            {
                Text = x.Name.ToUpper(),
                Value = x.Id.ToString()
            });

            IEnumerable<SelectListItem> engineAndFuelType = Enum.GetValues(typeof(EngineAndFuelType))
                .Cast<EngineAndFuelType>()
                .Select(x => new SelectListItem
                {
                    Text = x.ToString().ToUpper(),
                    Value = ((int)x).ToString()
                });

            IEnumerable<SelectListItem> transmission = Enum.GetValues(typeof(Transmission))
                .Cast<Transmission>()
                .Select(x => new SelectListItem
                {
                    Text = x.ToString().ToUpper(),
                    Value = ((int)x).ToString()
                });

            // Create PostVM with dropdown data
            PostVM postVM = new PostVM
            {
                Post = post,
                BrandList = brandList,
                VehicleTypeList = vehicleTypeList,
                EngineAndFuelTypeList = engineAndFuelType,
                TransmissionList = transmission
            };

            return View(postVM);
        }


        [HttpPost]
        public async Task<IActionResult> Delete(PostVM postVM)
        {
            var postFromDb = await _unitOfWork.Post.GetPostById(postVM.Post.Id); // Get from DB

            if (postFromDb == null)
            {
                return NotFound();
            }

            string WebRootPath = _webHostEnvironment.WebRootPath;
            if (!string.IsNullOrEmpty(postFromDb.VehicleImage))
            {
                string oldImagePath = Path.Combine(WebRootPath, postFromDb.VehicleImage.Trim('\\'));
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            await _unitOfWork.Post.Delete(postFromDb); // Delete the retrieved entity
            await _unitOfWork.SaveAsync();

            TempData["Success"] = CommonMessage.RecordDelete;
            return RedirectToAction(nameof(Index));
        }

        
    }
}
       







    

