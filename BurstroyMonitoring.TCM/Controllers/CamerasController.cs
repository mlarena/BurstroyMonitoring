using Microsoft.AspNetCore.Mvc;

namespace BurstroyMonitoring.TCM.Controllers
{
    public class CamerasController : Controller
    {
        // Список камер
        public IActionResult Index()
        {
            return View();
        }

        // Просмотр конкретной камеры и управление
        public IActionResult Details(int id)
        {
            ViewBag.CameraId = id;
            return View();
        }

        // Создание/подключение новой камеры
        public IActionResult Create()
        {
            return View();
        }

        // Редактирование настроек камеры
        public IActionResult Edit(int id)
        {
            return View();
        }

        // Удаление камеры
        public IActionResult Delete(int id)
        {
            return View();
        }
    }
}
