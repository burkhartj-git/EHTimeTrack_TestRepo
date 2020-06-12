using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using EH.TimeTrackNet.Web.Filters;
using EH.TimeTrackNet.Web.Models;
using EH.TimeTrackNet.Web.Repositories;
using EH.TimeTrackNet.Web.Utilities;
using EH.TimeTrackNet.Web.ViewModels;

namespace EH.TimeTrackNet.Web.Controllers
{
    [MacAuthorize(Action = "ViewPublished")]
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    public class AdminController : BaseController
    {
        private GenericEHTimetrackUnitOfWork uow = null;
        private ActivityGet activityRepo = null;
        private OptionGet optionRepo = null;
        private ProgramGet programRepo = null;
        private PurgeActivityDataGet purgeRepo = null;
        private SampleGet sampleRepo = null;
        private SubprogramGet subprogramRepo = null;
        private WorkerGet workerRepo = null;
        private string modifiedBy = null;

        private Entities db = new Entities();

        public AdminController()
        {
            uow = new GenericEHTimetrackUnitOfWork();
            activityRepo = new ActivityGet();
            optionRepo = new OptionGet();
            programRepo = new ProgramGet();
            purgeRepo = new PurgeActivityDataGet();
            sampleRepo = new SampleGet();
            subprogramRepo = new SubprogramGet();
            workerRepo = new WorkerGet();

            if (SessionHelper.UserName == null)
            {
                modifiedBy = "Test";
            }
            else
            {
                modifiedBy = SessionHelper.UserName;
            }
        }

        #region - Activities -

        [MacAuthorize(Action = "AdministerAll,AdministerOwn")]
        [HttpGet]
        public ActionResult MaintainActivities(bool DisplayEditMessage = false)
        {
            try
            {
                var model = new MaintainActivitiesViewModel();
                if (DisplayEditMessage == true)
                {
                    model.ShowEditMessage = true;
                    model.EditMessage = "Please select an activity";
                }
                List<REF_ACTIVITY_TYPE_TB> activities = uow.Repository<REF_ACTIVITY_TYPE_TB>().GetAll().ToList();
                foreach (var activity in activities.OrderBy(c => Int32.Parse(c.SZ_CODE)))
                {
                    var editorVM = new MaintainActivitiesEditorViewModel()
                    {
                        Id = activity.N_ACTIVITY_TYPE_SYSID,
                        Code = activity.SZ_CODE,
                        Description = activity.SZ_DESCRIPTION,
                        Selected = false
                    };
                    model.Activity.Add(editorVM);
                }
                return View(model);
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: AdminController.MaintainActivities_GET\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: AdminController.MaintainActivities_GET\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        [MacAuthorize(Action = "AdministerAll,AdministerOwn")]
        [HttpPost]
        public ActionResult MaintainActivitiesSubmitSelected(MaintainActivitiesViewModel model, string Action)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    IEnumerable<int> selectedIds = model.getSelectedIds();
                    int id = selectedIds.FirstOrDefault();
                    if (id > 0)
                    {
                        REF_ACTIVITY_TYPE_TB activity = uow.Repository<REF_ACTIVITY_TYPE_TB>().GetById(id);

                        if (Action == "Edit")
                        { 
                            var editActivitiesModel = new EditActivitiesViewModel()
                            {
                                Id = activity.N_ACTIVITY_TYPE_SYSID,
                                Code = Convert.ToInt32(activity.SZ_CODE),
                                Description = activity.SZ_DESCRIPTION,
                                IsNew = false,
                                ShowMessageCode = false,
                                ShowMessageDescription = false,
                                ValidationMessage = ""
                            };
                            return View("EditActivities", editActivitiesModel);
                        }
                        else //Action == "Delete"
                        {
                            var deleteActivitiesModel = new DeleteActivitiesViewModel()
                            {
                                Id = activity.N_ACTIVITY_TYPE_SYSID,
                                Code = Convert.ToInt32(activity.SZ_CODE),
                                Description = activity.SZ_DESCRIPTION,
                                Message = "",
                                ShowMessage = false
                            };
                            return View("DeleteActivities", deleteActivitiesModel);
                        }
                    }
                    else
                    {
                        return RedirectToAction("MaintainActivities", new { DisplayEditMessage = true });
                    }
                }

                return View("MaintainActivities");
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: AdminController.MaintainActivitiesSubmitSelected_POST\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: AdminController.MaintainActivitiesSubmitSelected_POST\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        [MacAuthorize(Action = "AdministerAll,AdministerOwn")]
        [HttpGet]
        public ActionResult EditActivities(EditActivitiesViewModel model)
        {
            try
            {
                return View(model);
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: AdminController.EditActivities_GET\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: AdminController.EditActivities_GET\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        [MacAuthorize(Action = "AdministerAll,AdministerOwn")]
        [HttpGet]
        public ActionResult DeleteActivities(DeleteActivitiesViewModel model)
        {
            try
            {
                return View(model);
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: AdminController.DeleteActivities_GET\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: AdminController.DeleteActivities_GET\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        [MacAuthorize(Action = "AdministerAll,AdministerOwn")]
        [HttpPost]
        public ActionResult DeleteActivity(DeleteActivitiesViewModel model)
        {
            try
            {
                int id = Convert.ToInt32(model.Id);
                if (uow.Repository<TRN_SERVICE_X_ACTIVITY_TYPE_TB>().Find(u => u.N_ACTIVITY_TYPE_SYSID == id).Count() > 0)
                {
                    model.ShowMessage = true;
                    model.Message = "Service Log records exist for this activity. Sorry, the activity can not be deleted.";

                    return RedirectToAction("DeleteActivities", model);
                }
                else
                {
                    uow.Repository<REF_ACTIVITY_TYPE_TB>().Delete(id);
                    uow.SaveChanges();

                    return RedirectToAction("MaintainActivities");
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: AdminController.DeleteActivity_GET\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: AdminController.DeleteActivity_GET\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        [MacAuthorize(Action = "AdministerAll,AdministerOwn")]
        [HttpPost]
        public ActionResult InsertActivitiesSubmitSelected()
        {
            try
            {
                var editActivitiesModel = new EditActivitiesViewModel()
                {
                    Id = 0,
                    Code = activityRepo.GetNextActivityCode(),
                    Description = "",
                    IsNew = true,
                    ShowMessageCode = false,
                    ShowMessageDescription = false,
                    ValidationMessage = ""
                };
                return View("EditActivities", editActivitiesModel);
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: AdminController.InsertActivitiesSubmitSelected_POST\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: AdminController.InsertActivitiesSubmitSelected_POST\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        [MacAuthorize(Action = "AdministerAll,AdministerOwn")]
        [HttpPost]
        public ActionResult EditActivitiesSubmitSelected(EditActivitiesViewModel model)
        {
            bool saveFailed;
            do
            {
                saveFailed = false;
                try
                {
                    if (ModelState["Code"].Errors.Count > 0)
                    {
                        string code = ModelState["Code"].Value.AttemptedValue.ToString();
                        string[] parts = code.Split(',');
                        string entry = parts[0];
                        string response = "The value: '" + entry + "' is not valid. Please enter a numeric value.";
                        ModelState.Remove("Code");
                        ModelState.AddModelError("Code", response);
                    }

                    if (ModelState.IsValid)
                    {
                        //bool isActivityNumberDuplicate = false;
                        bool isActivityDescriptionDuplicate = false;

                        //if (model.Id == 0)
                        //{
                        //    isActivityNumberDuplicate = activityRepo.IsActivityNumberDuplicate(model.Code);
                        //}

                        isActivityDescriptionDuplicate = activityRepo.IsActivityDescriptionDuplicate(model.Description);

                        //if (isActivityNumberDuplicate)
                        //{
                        //    model.IsNew = true;
                        //    model.ShowMessageCode = true;
                        //    model.ValidationMessage = "The activity number already exists. Please enter different one.";
                        //
                        //    return View("EditActivities", model);
                        //}
                        if (isActivityDescriptionDuplicate)
                        {
                            model.ShowMessageDescription = true;
                            model.ValidationMessage = "The activity description already exists. Please enter different one.";

                            return View("EditActivities", model);
                        }
                        else
                        {
                            string modifiedBy = null;
                            if (SessionHelper.UserName == null)
                            {
                                modifiedBy = "Test";
                            }
                            else
                            {
                                modifiedBy = SessionHelper.UserName;
                            }
                            if (model.Id == 0)
                            {
                                REF_ACTIVITY_TYPE_TB activity = new REF_ACTIVITY_TYPE_TB()
                                {
                                    B_INACTIVE = false,
                                    DT_MODIFIED = DateTime.UtcNow,
                                    SZ_CODE = model.Code.ToString(),
                                    SZ_DESCRIPTION = model.Description,
                                    SZ_MODIFIED_BY = modifiedBy
                                };
                                uow.Repository<REF_ACTIVITY_TYPE_TB>().Add(activity);
                            }
                            else
                            {
                                REF_ACTIVITY_TYPE_TB activity = uow.Repository<REF_ACTIVITY_TYPE_TB>().GetById(model.Id);
                                activity.DT_MODIFIED = DateTime.UtcNow;
                                activity.SZ_CODE = model.Code.ToString();
                                activity.SZ_DESCRIPTION = model.Description;
                                activity.SZ_MODIFIED_BY = modifiedBy;
                                uow.Repository<REF_ACTIVITY_TYPE_TB>().Update(activity);
                            }
                            uow.SaveChanges();
                            return RedirectToAction("MaintainActivities");
                        }
                    }
                    model.ShowMessageCode = false;
                    model.ShowMessageDescription = false;
                    model.ValidationMessage = "";
                    model.IsNew = true;

                    return View("EditActivities", model);
                }
                catch (DbEntityValidationException ex)
                {
                    StringBuilder sb = new StringBuilder();

                    foreach (var failure in ex.EntityValidationErrors)
                    {
                        sb.AppendFormat("{0} failed validation:\n\n", failure.Entry.Entity.GetType());
                        foreach (var error in failure.ValidationErrors)
                        {
                            sb.AppendFormat("- {0} : {1}", error.PropertyName, error.ErrorMessage);
                            sb.AppendLine();
                        }
                    }
                    if (ex.InnerException == null)
                    {
                        ViewBag.Message = "Function: AdminController.EditActivitiesSubmitSelected_POST\n\nError: " + ex.Message + "\n\n" + sb;
                    }
                    else
                    {
                        ViewBag.Message = "Function: AdminController.EditActivitiesSubmitSelected_POST\n\nError: " + ex.Message + "\n\n" + sb + "\n\n" + ex.InnerException.Message;
                    };
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    saveFailed = true;

                    var entry = ex.Entries.Single();
                    entry.OriginalValues.SetValues(entry.GetDatabaseValues());
                }
                catch (DataException ex)
                {
                    if (ex.InnerException == null)
                    {
                        ViewBag.Message = "Function: AdminController.EditActivitiesSubmitSelected_POST\n\nError: " + ex.Message + "\n\nBaseException: " + ex.GetBaseException().Message;
                    }
                    else
                    {
                        ViewBag.Message = "Function: AdminController.EditActivitiesSubmitSelected_POST\n\nError: " + ex.Message + "\n\nBaseException: " + ex.GetBaseException().Message + "\n\nInnerException: " + ex.InnerException.Message;
                    };
                }
                catch (Exception ex)
                {
                    if (ex.InnerException == null)
                    {
                        ViewBag.Message = "Function: AdminController.EditActivitiesSubmitSelected_POST\n\nError: " + ex.Message;
                    }
                    else
                    {
                        ViewBag.Message = "Function: AdminController.EditActivitiesSubmitSelected_POST\n\nError: " + ex.Message + "\n\nBaseException: " + ex.InnerException.Message;
                    };
                };
            } while (saveFailed);

            Session["ErrorMessage"] = ViewBag.Message;
            return RedirectToAction("InternalServerError", "Error");
        }

        #endregion

        #region - Options -

        [MacAuthorize(Action = "AdministerAll,AdministerOwn")]
        [HttpGet]
        public ActionResult MaintainOptions(bool DisplayEditMessage = false)
        {
            try
            {
                var model = new MaintainOptionsViewModel();
                if (DisplayEditMessage == true)
                {
                    model.ShowEditMessage = true;
                    model.EditMessage = "Please select an option";
                }
                List<REF_OPTION_TB> options = uow.Repository<REF_OPTION_TB>().GetAll().ToList();
                foreach (var option in options.OrderBy(c => Int32.Parse(c.SZ_CODE)))
                {
                    var editorVM = new MaintainOptionsEditorViewModel()
                    {
                        Id = option.N_OPTION_SYSID,
                        Code = option.SZ_CODE,
                        Description = option.SZ_DESCRIPTION,
                        Selected = false
                    };
                    model.Option.Add(editorVM);
                }
                return View(model);
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: AdminController.MaintainOptions_GET\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: AdminController.MaintainOptions_GET\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        [MacAuthorize(Action = "AdministerAll,AdministerOwn")]
        [HttpPost]
        public ActionResult MaintainOptionsSubmitSelected(MaintainOptionsViewModel model, string Action)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    IEnumerable<int> selectedIds = model.getSelectedIds();
                    int id = selectedIds.FirstOrDefault();
                    if (id > 0)
                    {
                        REF_OPTION_TB option = uow.Repository<REF_OPTION_TB>().GetById(id);

                        if (Action == "Edit")
                        {
                            var editOptionsModel = new EditOptionsViewModel()
                            {
                                Id = option.N_OPTION_SYSID,
                                Code = Convert.ToInt32(option.SZ_CODE),
                                Description = option.SZ_DESCRIPTION,
                                IsNew = false,
                                ShowMessageCode = false,
                                ShowMessageDescription = false,
                                ValidationMessage = ""
                            };
                            return View("EditOptions", editOptionsModel);
                        }
                        else //Action == "Delete"
                        {
                            var deleteOptionsModel = new DeleteOptionsViewModel()
                            {
                                Id = option.N_OPTION_SYSID,
                                Code = Convert.ToInt32(option.SZ_CODE),
                                Description = option.SZ_DESCRIPTION,
                                Message = "",
                                ShowMessage = false
                            };
                            return View("DeleteOptions", deleteOptionsModel);
                        }
                    }
                    else
                    {
                        return RedirectToAction("MaintainOptions", new { DisplayEditMessage = true });
                    }
                }
                return View("MaintainOptions");
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: AdminController.MaintainOptionsSubmitSelected_POST\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: AdminController.MaintainOptionsSubmitSelected_POST\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        [MacAuthorize(Action = "AdministerAll,AdministerOwn")]
        [HttpGet]
        public ActionResult EditOptions(EditOptionsViewModel model)
        {
            try
            {
                return View(model);
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: AdminController.EditOptions_GET\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: AdminController.EditOptions_GET\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        [MacAuthorize(Action = "AdministerAll,AdministerOwn")]
        [HttpPost]
        public ActionResult InsertOptionsSubmitSelected()
        {
            try
            {
                var editOptionsModel = new EditOptionsViewModel()
                {
                    Id = 0,
                    Code = optionRepo.GetNextOptionCode(),
                    Description = "",
                    IsNew = true,
                    ShowMessageCode = false,
                    ShowMessageDescription = false,
                    ValidationMessage = ""
                };
                return View("EditOptions", editOptionsModel);
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: AdminController.InsertOptionsSubmitSelected_POST\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: AdminController.InsertOptionsSubmitSelected_POST\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        [MacAuthorize(Action = "AdministerAll,AdministerOwn")]
        [HttpPost]
        public ActionResult EditOptionsSubmitSelected(EditOptionsViewModel model)
        {
            bool saveFailed;
            do
            {
                saveFailed = false;
                try
                {
                    if (ModelState["Code"].Errors.Count > 0)
                    {
                        string code = ModelState["Code"].Value.AttemptedValue.ToString();
                        string[] parts = code.Split(',');
                        string entry = parts[0];
                        string response = "The value: '" + entry + "' is not valid. Please enter a numeric value.";
                        ModelState.Remove("Code");
                        ModelState.AddModelError("Code", response);
                    }

                    if (ModelState.IsValid)
                    {
                        //bool isOptionNumberDuplicate = false;
                        bool isOptionDescriptionDuplicate = false;

                        //if (model.Id == 0)
                        //{
                        //    isOptionNumberDuplicate = optionRepo.IsOptionNumberDuplicate(model.Code);
                        //}

                        isOptionDescriptionDuplicate = optionRepo.IsOptionDescriptionDuplicate(model.Description);

                        //if (isOptionNumberDuplicate)
                        //{
                        //    model.IsNew = true;
                        //    model.ShowMessageCode = true;
                        //    model.ValidationMessage = "The option number already exists. Please enter different one.";
                        //
                        //    return View("EditOptions", model);
                        //}
                        if (isOptionDescriptionDuplicate)
                        {
                            model.ShowMessageDescription = true;
                            model.ValidationMessage = "The option description already exists. Please enter different one.";

                            return View("EditOptions", model);
                        }
                        else
                        {
                            string modifiedBy = null;
                            if (SessionHelper.UserName == null)
                            {
                                modifiedBy = "Test";
                            }
                            else
                            {
                                modifiedBy = SessionHelper.UserName;
                            }
                            if (model.Id == 0)
                            {
                                REF_OPTION_TB option = new REF_OPTION_TB()
                                {
                                    B_INACTIVE = false,
                                    DT_MODIFIED = DateTime.UtcNow,
                                    SZ_CODE = model.Code.ToString(),
                                    SZ_DESCRIPTION = model.Description,
                                    SZ_MODIFIED_BY = modifiedBy
                                };
                                uow.Repository<REF_OPTION_TB>().Add(option);
                            }
                            else
                            {
                                REF_OPTION_TB option = uow.Repository<REF_OPTION_TB>().GetById(model.Id);
                                option.DT_MODIFIED = DateTime.UtcNow;
                                option.SZ_CODE = model.Code.ToString();
                                option.SZ_DESCRIPTION = model.Description;
                                option.SZ_MODIFIED_BY = modifiedBy;
                                uow.Repository<REF_OPTION_TB>().Update(option);
                            }
                            uow.SaveChanges();
                            return RedirectToAction("MaintainOptions");
                        }
                    }
                    model.ShowMessageCode = false;
                    model.ShowMessageDescription = false;
                    model.ValidationMessage = "";
                    model.IsNew = true;

                    return View("EditOptions", model);
                }
                catch (DbEntityValidationException ex)
                {
                    StringBuilder sb = new StringBuilder();

                    foreach (var failure in ex.EntityValidationErrors)
                    {
                        sb.AppendFormat("{0} failed validation:\n\n", failure.Entry.Entity.GetType());
                        foreach (var error in failure.ValidationErrors)
                        {
                            sb.AppendFormat("- {0} : {1}", error.PropertyName, error.ErrorMessage);
                            sb.AppendLine();
                        }
                    }
                    if (ex.InnerException == null)
                    {
                        ViewBag.Message = "Function: AdminController.EditOptionsSubmitSelected_POST\n\nError: " + ex.Message + "\n\n" + sb;
                    }
                    else
                    {
                        ViewBag.Message = "Function: AdminController.EditOptionsSubmitSelected_POST\n\nError: " + ex.Message + "\n\n" + sb + "\n\n" + ex.InnerException.Message;
                    };
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    saveFailed = true;

                    var entry = ex.Entries.Single();
                    entry.OriginalValues.SetValues(entry.GetDatabaseValues());
                }
                catch (DataException ex)
                {
                    if (ex.InnerException == null)
                    {
                        ViewBag.Message = "Function: AdminController.EditOptionsSubmitSelected_POST\n\nError: " + ex.Message + "\n\nBaseException: " + ex.GetBaseException().Message;
                    }
                    else
                    {
                        ViewBag.Message = "Function: AdminController.EditOptionsSubmitSelected_POST\n\nError: " + ex.Message + "\n\nBaseException: " + ex.GetBaseException().Message + "\n\nInnerException: " + ex.InnerException.Message;
                    };
                }
                catch (Exception ex)
                {
                    if (ex.InnerException == null)
                    {
                        ViewBag.Message = "Function: AdminController.EditOptionsSubmitSelected_POST\n\nError: " + ex.Message;
                    }
                    else
                    {
                        ViewBag.Message = "Function: AdminController.EditOptionsSubmitSelected_POST\n\nError: " + ex.Message + "\n\nBaseException: " + ex.InnerException.Message;
                    };
                };
            } while (saveFailed);

            Session["ErrorMessage"] = ViewBag.Message;
            return RedirectToAction("InternalServerError", "Error");
        }

        [MacAuthorize(Action = "AdministerAll,AdministerOwn")]
        [HttpGet]
        public ActionResult DeleteOptions(DeleteOptionsViewModel model)
        {
            try
            {
                return View(model);
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: AdminController.DeleteOptions_GET\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: AdminController.DeleteOptions_GET\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        [MacAuthorize(Action = "AdministerAll,AdministerOwn")]
        [HttpPost]
        public ActionResult DeleteOption(DeleteOptionsViewModel model)
        {
            try
            {
                int id = Convert.ToInt32(model.Id);
                if (uow.Repository<TRN_SERVICE_X_ACTIVITY_TYPE_TB>().Find(u => u.N_OPTION_SYSID == id).Count() > 0)
                {
                    model.ShowMessage = true;
                    model.Message = "Service Log records exist for this option. Sorry, the option can not be deleted.";

                    return RedirectToAction("DeleteOptions", model);
                }
                else
                {
                    uow.Repository<REF_OPTION_TB>().Delete(id);
                    uow.SaveChanges();

                    return RedirectToAction("MaintainOptions");
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: AdminController.DeleteOption_GET\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: AdminController.DeleteOption_GET\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        #endregion

        #region - Programs -

        [MacAuthorize(Action = "AdministerAll,AdministerOwn")]
        [HttpGet]
        public ActionResult MaintainPrograms(bool DisplayEditMessage = false)
        {
            try
            {
                var model = new MaintainProgramsViewModel();
                if (DisplayEditMessage == true)
                {
                    model.ShowEditMessage = true;
                    model.EditMessage = "Please select a program";
                }
                List<REF_PROGRAM_TB> programs = uow.Repository<REF_PROGRAM_TB>().GetAll().ToList();
                foreach (var program in programs.OrderBy(c => Int32.Parse(c.SZ_CODE)))
                {
                    var editorVM = new MaintainProgramsEditorViewModel()
                    {
                        Id = program.N_PROGRAM_SYSID,
                        Code = program.SZ_CODE,
                        Description = program.SZ_DESCRIPTION,
                        Selected = false
                    };
                    model.Program.Add(editorVM);
                }
                return View(model);
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: AdminController.MaintainPrograms_GET\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: AdminController.MaintainPrograms_GET\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        [MacAuthorize(Action = "AdministerAll,AdministerOwn")]
        [HttpPost]
        public ActionResult MaintainProgramsSubmitSelected(MaintainProgramsViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    IEnumerable<int> selectedIds = model.getSelectedIds();
                    int id = selectedIds.FirstOrDefault();
                    string str = Request.Params["btnEdit"];
                    if (str == "Subprogram")
                    {
                        if (id > 0)
                        {
                            return RedirectToAction("MaintainSubprogramsHelper", new { ID = id });
                        }
                        else
                        {
                            return RedirectToAction("MaintainPrograms", new { DisplayEditMessage = true });
                        }
                    }
                    else 
                    {
                        if (id > 0)
                        {
                            REF_PROGRAM_TB program = uow.Repository<REF_PROGRAM_TB>().GetById(id);

                            if (str == "Edit")
                            {
                                var editProgramsModel = new EditProgramsViewModel()
                                {
                                    Id = program.N_PROGRAM_SYSID,
                                    Code = Convert.ToInt32(program.SZ_CODE),
                                    Description = program.SZ_DESCRIPTION,
                                    Sections = (List<SelectListItem>)programRepo.GetSections(),
                                    SelectedSection = program.N_SECTION_SYSID,
                                    IsNew = false,
                                    ShowMessageCode = false,
                                    ShowMessageDescription = false,
                                    ValidationMessage = ""
                                };
                                return View("EditPrograms", editProgramsModel);
                            }
                            else //str == "Delete"
                            {
                                var deleteProgramsModel = new DeleteProgramsViewModel()
                                {
                                    Id = program.N_PROGRAM_SYSID,
                                    Code = Convert.ToInt32(program.SZ_CODE),
                                    Description = program.SZ_DESCRIPTION,
                                    Message = "",
                                    ShowMessage = false
                                };
                                return View("DeletePrograms", deleteProgramsModel);
                            }
                        }
                        else
                        {
                            return RedirectToAction("MaintainPrograms", new { DisplayEditMessage = true });
                        }
                    }
                }
                return View("MaintainPrograms", model);
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: AdminController.MaintainProgramsSubmitSelected_POST\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: AdminController.MaintainProgramsSubmitSelected_POST\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        [MacAuthorize(Action = "AdministerAll,AdministerOwn")]
        [HttpGet]
        public ActionResult EditPrograms(EditProgramsViewModel model)
        {
            try
            {
                return View(model);
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: AdminController.EditPrograms_GET\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: AdminController.EditPrograms_GET\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        [MacAuthorize(Action = "AdministerAll,AdministerOwn")]
        [HttpPost]
        public ActionResult InsertProgramsSubmitSelected()
        {
            try
            {
                var editProgramsModel = new EditProgramsViewModel()
                {
                    Id = 0,
                    Code = programRepo.GetNextProgramCode(),
                    Description = "",
                    IsNew = true,
                    ShowMessageCode = false,
                    ShowMessageDescription = false,
                    ValidationMessage = "",
                    Sections = (List<SelectListItem>)programRepo.GetSections(),
                    SelectedSection = 0
                };
                return View("EditPrograms", editProgramsModel);
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: AdminController.InsertProgramsSubmitSelected_POST\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: AdminController.InsertProgramsSubmitSelected_POST\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        [MacAuthorize(Action = "AdministerAll,AdministerOwn")]
        [HttpPost]
        public ActionResult EditProgramsSubmitSelected(EditProgramsViewModel model)
        {
            bool saveFailed;
            do
            {
                saveFailed = false;
                try
                {
                    if (ModelState["Code"].Errors.Count > 0)
                    {
                        string code = ModelState["Code"].Value.AttemptedValue.ToString();
                        string[] parts = code.Split(',');
                        string entry = parts[0];
                        string response = "The value: '" + entry + "' is not valid. Please enter a numeric value.";
                        ModelState.Remove("Code");
                        ModelState.AddModelError("Code", response);
                    }

                    if (ModelState.IsValid)
                    {
                        //bool isProgramNumberDuplicate = false;
                        bool isProgramDescriptionDuplicate = false;

                        //if (model.Id == 0)
                        //{
                        //    isProgramNumberDuplicate = programRepo.IsProgramNumberDuplicate(model.Code);
                        //}

                        isProgramDescriptionDuplicate = programRepo.IsProgramDescriptionDuplicate(model.Description);

                        //if (isProgramNumberDuplicate)
                        //{
                        //    model.IsNew = true;
                        //    model.ShowMessageCode = true;
                        //    model.ValidationMessage = "The program number already exists. Please enter different one.";
                        //    model.Sections = (List<SelectListItem>)programRepo.GetSections();
                        //
                        //    return View("EditPrograms", model);
                        //}
                        if (isProgramDescriptionDuplicate)
                        {
                            model.ShowMessageDescription = true;
                            model.ValidationMessage = "The program description already exists. Please enter different one.";
                            model.Sections = (List<SelectListItem>)programRepo.GetSections();

                            return View("EditPrograms", model);
                        }
                        else
                        {
                            if (model.Id == 0)
                            {
                                REF_PROGRAM_TB program = new REF_PROGRAM_TB()
                                {
                                    B_INACTIVE = false,
                                    DT_MODIFIED = DateTime.UtcNow,
                                    SZ_CODE = model.Code.ToString(),
                                    SZ_DESCRIPTION = model.Description,
                                    N_SECTION_SYSID = 1,
                                    SZ_MODIFIED_BY = modifiedBy
                                };
                                uow.Repository<REF_PROGRAM_TB>().Add(program);
                                uow.SaveChanges();

                                REF_PROGRAM_TB programFound = uow.Repository<REF_PROGRAM_TB>().Find(u => u.SZ_CODE == model.Code.ToString()
                                                                                                    && u.SZ_DESCRIPTION == model.Description
                                                                                                    && u.N_SECTION_SYSID == model.SelectedSection
                                                                                                    && u.SZ_MODIFIED_BY == modifiedBy).FirstOrDefault();

                                //REF_SUBPROGRAM_TB subprogram = new REF_SUBPROGRAM_TB()
                                //{
                                //    B_INACTIVE = false,
                                //    DT_MODIFIED = DateTime.UtcNow,
                                //    N_PROGRAM_SYSID = programFound.N_PROGRAM_SYSID,
                                //    SZ_CODE = "0",
                                //    SZ_DESCRIPTION = "Not Applicable",
                                //    SZ_MODIFIED_BY = modifiedBy
                                //};
                                //uow.Repository<REF_SUBPROGRAM_TB>().Add(subprogram);
                                //uow.SaveChanges();
                            }
                            else
                            {
                                REF_PROGRAM_TB program = uow.Repository<REF_PROGRAM_TB>().GetById(model.Id);
                                program.DT_MODIFIED = DateTime.UtcNow;
                                program.SZ_CODE = model.Code.ToString();
                                program.SZ_DESCRIPTION = model.Description;
                                program.N_SECTION_SYSID = 1;
                                program.SZ_MODIFIED_BY = modifiedBy;
                                uow.Repository<REF_PROGRAM_TB>().Update(program);
                                uow.SaveChanges();
                            }

                            return RedirectToAction("MaintainPrograms");
                        }
                    }
                    model.ShowMessageCode = false;
                    model.ShowMessageDescription = false;
                    model.ValidationMessage = "";
                    model.IsNew = true;
                    model.Sections = (List<SelectListItem>)programRepo.GetSections();

                    return View("EditPrograms", model);
                }
                catch (DbEntityValidationException ex)
                {
                    StringBuilder sb = new StringBuilder();

                    foreach (var failure in ex.EntityValidationErrors)
                    {
                        sb.AppendFormat("{0} failed validation:\n\n", failure.Entry.Entity.GetType());
                        foreach (var error in failure.ValidationErrors)
                        {
                            sb.AppendFormat("- {0} : {1}", error.PropertyName, error.ErrorMessage);
                            sb.AppendLine();
                        }
                    }
                    if (ex.InnerException == null)
                    {
                        ViewBag.Message = "Function: AdminController.EditProgramsSubmitSelected_POST\n\nError: " + ex.Message + "\n\n" + sb;
                    }
                    else
                    {
                        ViewBag.Message = "Function: AdminController.EditProgramsSubmitSelected_POST\n\nError: " + ex.Message + "\n\n" + sb + "\n\n" + ex.InnerException.Message;
                    };
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    saveFailed = true;

                    var entry = ex.Entries.Single();
                    entry.OriginalValues.SetValues(entry.GetDatabaseValues());
                }
                catch (DataException ex)
                {
                    if (ex.InnerException == null)
                    {
                        ViewBag.Message = "Function: AdminController.EditProgramsSubmitSelected_POST\n\nError: " + ex.Message + "\n\nBaseException: " + ex.GetBaseException().Message;
                    }
                    else
                    {
                        ViewBag.Message = "Function: AdminController.EditProgramsSubmitSelected_POST\n\nError: " + ex.Message + "\n\nBaseException: " + ex.GetBaseException().Message + "\n\nInnerException: " + ex.InnerException.Message;
                    };
                }
                catch (Exception ex)
                {
                    if (ex.InnerException == null)
                    {
                        ViewBag.Message = "Function: AdminController.EditProgramsSubmitSelected_POST\n\nError: " + ex.Message;
                    }
                    else
                    {
                        ViewBag.Message = "Function: AdminController.EditProgramsSubmitSelected_POST\n\nError: " + ex.Message + "\n\nBaseException: " + ex.InnerException.Message;
                    };
                };
            } while (saveFailed);

            Session["ErrorMessage"] = ViewBag.Message;
            return RedirectToAction("InternalServerError", "Error");
        }

        [MacAuthorize(Action = "AdministerAll,AdministerOwn")]
        [HttpGet]
        public ActionResult DeletePrograms(DeleteProgramsViewModel model)
        {
            try
            {
                return View(model);
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: AdminController.DeletePrograms_GET\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: AdminController.DeletePrograms_GET\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        [MacAuthorize(Action = "AdministerAll,AdministerOwn")]
        [HttpPost]
        public ActionResult DeleteProgram(DeleteProgramsViewModel model)
        {
            try
            {
                int id = Convert.ToInt32(model.Id);
                if (uow.Repository<TRN_SERVICE_X_ACTIVITY_TYPE_TB>().Find(u => u.N_PROGRAM_SYSID == id).Count() > 0)
                {
                    model.ShowMessage = true;
                    model.Message = "Service Log records exist for this program. Sorry, the program can not be deleted.";

                    return RedirectToAction("DeletePrograms", model);
                }
                else if (uow.Repository<REF_SUBPROGRAM_TB>().Find(u => u.N_PROGRAM_SYSID == id).Count() > 0)
                {
                    model.ShowMessage = true;
                    model.Message = "Subprograms exist for this program. Sorry, the program can not be deleted.";

                    return RedirectToAction("DeletePrograms", model);
                }
                else
                {
                    uow.Repository<REF_PROGRAM_TB>().Delete(id);
                    uow.SaveChanges();

                    return RedirectToAction("MaintainPrograms");
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: AdminController.DeleteProgram_GET\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: AdminController.DeleteProgram_GET\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        #endregion

        #region - Purge Activity Data -

        [MacAuthorize(Action = "AdministerAll,AdministerOwn")]
        [HttpGet]
        public ActionResult PurgeActivityData()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: AdminController.PurgeActivityData_GET\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: AdminController.PurgeActivityData_GET\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        [MacAuthorize(Action = "AdministerAll,AdministerOwn")]
        [HttpGet]
        public ActionResult PurgeActivityDataByDate(string PurgeDate)
        {
            try
            {
                bool isPurged = purgeRepo.IsPurged(Convert.ToDateTime(PurgeDate));
                return RedirectToAction("PurgeActivityDataConfirm");
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: AdminController.PurgeActivityDataByDate_GET\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: AdminController.PurgeActivityDataByDate_GET\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        #endregion         

        #region - Reports -

        [MacAuthorize(Action = "ViewPublished")]
        [HttpGet]
        public ActionResult Reports()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: AdminController.Reports_GET\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: AdminController.Reports_GET\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        #endregion

        #region - Samples -

        [MacAuthorize(Action = "AdministerAll,AdministerOwn")]
        [HttpGet]
        public ActionResult MaintainSamples(bool DisplayEditMessage = false)
        {
            try
            {
                var model = new MaintainSamplesViewModel();
                if (DisplayEditMessage == true)
                {
                    model.ShowEditMessage = true;
                    model.EditMessage = "Please select a sample";
                }
                List<REF_SAMPLE_TYPE_TB> samples = uow.Repository<REF_SAMPLE_TYPE_TB>().GetAll().ToList();
                foreach (var sample in samples.OrderBy(c => Int64.Parse(c.SZ_CODE)))
                {
                    var editorVM = new MaintainSamplesEditorViewModel()
                    {
                        Id = sample.N_SAMPLE_TYPE_SYSID,
                        Code = sample.SZ_CODE,
                        Description = sample.SZ_DESCRIPTION,
                        Selected = false
                    };
                    model.Sample.Add(editorVM);
                }
                return View(model);
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: AdminController.MaintainSamples_GET\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: AdminController.MaintainSamples_GET\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        [MacAuthorize(Action = "AdministerAll,AdministerOwn")]
        [HttpPost]
        public ActionResult MaintainSamplesSubmitSelected(MaintainSamplesViewModel model, string Action)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    IEnumerable<int> selectedIds = model.getSelectedIds();
                    int id = selectedIds.FirstOrDefault();
                    if (id > 0)
                    {
                        REF_SAMPLE_TYPE_TB sample = uow.Repository<REF_SAMPLE_TYPE_TB>().GetById(id);

                        if (Action == "Edit")
                        {
                            var editSamplesModel = new EditSamplesViewModel()
                            {
                                Id = sample.N_SAMPLE_TYPE_SYSID,
                                Code = Convert.ToInt32(sample.SZ_CODE),
                                Description = sample.SZ_DESCRIPTION,
                                IsNew = false,
                                ShowMessageCode = false,
                                ShowMessageDescription = false,
                                ValidationMessage = ""
                            };
                            return View("EditSamples", editSamplesModel);
                        }
                        else //Action == "Delete"
                        {
                            var deleteSamplesModel = new DeleteSamplesViewModel()
                            {
                                Id = sample.N_SAMPLE_TYPE_SYSID,
                                Code = Convert.ToInt32(sample.SZ_CODE),
                                Description = sample.SZ_DESCRIPTION,
                                Message = "",
                                ShowMessage = false
                            };
                            return View("DeleteSamples", deleteSamplesModel);
                        }
                    }
                    else
                    {
                        return RedirectToAction("MaintainSamples", new { DisplayEditMessage = true });
                    }
                }
                return View("MaintainSamples");
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: AdminController.MaintainSamplesSubmitSelected_POST\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: AdminController.MaintainSamplesSubmitSelected_POST\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        [MacAuthorize(Action = "AdministerAll,AdministerOwn")]
        [HttpGet]
        public ActionResult EditSamples(EditSamplesViewModel model)
        {
            try
            {
                return View(model);
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: AdminController.EditSamples_GET\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: AdminController.EditSamples_GET\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        [MacAuthorize(Action = "AdministerAll,AdministerOwn")]
        [HttpPost]
        public ActionResult InsertSamplesSubmitSelected()
        {
            try
            {
                var editSamplesModel = new EditSamplesViewModel()
                {
                    Id = 0,
                    Code = sampleRepo.GetNextSampleCode(),
                    Description = "",
                    IsNew = true,
                    ShowMessageCode = false,
                    ShowMessageDescription = false,
                    ValidationMessage = ""
                };
                return View("EditSamples", editSamplesModel);
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: AdminController.InsertSamplesSubmitSelected_POST\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: AdminController.InsertSamplesSubmitSelected_POST\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        [MacAuthorize(Action = "AdministerAll,AdministerOwn")]
        [HttpPost]
        public ActionResult EditSamplesSubmitSelected(EditSamplesViewModel model)
        {
            bool saveFailed;
            do
            {
                saveFailed = false;
                try
                {
                    if (ModelState["Code"].Errors.Count > 0)
                    {
                        string code = ModelState["Code"].Value.AttemptedValue.ToString();
                        string[] parts = code.Split(',');
                        string entry = parts[0];
                        string response = "The value: '" + entry + "' is not valid. Please enter a numeric value.";
                        ModelState.Remove("Code");
                        ModelState.AddModelError("Code", response);
                    }

                    if (ModelState.IsValid)
                    {
                        //bool isSampleNumberDuplicate = false;
                        bool isSampleDescriptionDuplicate = false;

                        //if (model.Id == 0)
                        //{
                        //    isSampleNumberDuplicate = sampleRepo.IsSampleNumberDuplicate(model.Code);
                        //}

                        isSampleDescriptionDuplicate = sampleRepo.IsSampleDescriptionDuplicate(model.Description);

                        //if (isSampleNumberDuplicate)
                        //{
                        //    model.IsNew = true;
                        //    model.ShowMessageCode = true;
                        //    model.ValidationMessage = "The sample number already exists. Please enter different one.";
                        //
                        //    return View("EditSamples", model);
                        //}
                        if (isSampleDescriptionDuplicate)
                        {
                            model.ShowMessageDescription = true;
                            model.ValidationMessage = "The sample description already exists. Please enter different one.";

                            return View("EditSamples", model);
                        }
                        else
                        {
                            string modifiedBy = null;
                            if (SessionHelper.UserName == null)
                            {
                                modifiedBy = "Test";
                            }
                            else
                            {
                                modifiedBy = SessionHelper.UserName;
                            }
                            if (model.Id == 0)
                            {
                                REF_SAMPLE_TYPE_TB sample = new REF_SAMPLE_TYPE_TB()
                                {
                                    B_INACTIVE = false,
                                    DT_MODIFIED = DateTime.UtcNow,
                                    SZ_CODE = model.Code.ToString(),
                                    SZ_DESCRIPTION = model.Description,
                                    SZ_MODIFIED_BY = modifiedBy
                                };
                                uow.Repository<REF_SAMPLE_TYPE_TB>().Add(sample);
                            }
                            else
                            {
                                REF_SAMPLE_TYPE_TB sample = uow.Repository<REF_SAMPLE_TYPE_TB>().GetById(model.Id);
                                sample.DT_MODIFIED = DateTime.UtcNow;
                                sample.SZ_CODE = model.Code.ToString();
                                sample.SZ_DESCRIPTION = model.Description;
                                sample.SZ_MODIFIED_BY = modifiedBy;
                                uow.Repository<REF_SAMPLE_TYPE_TB>().Update(sample);
                            }
                            uow.SaveChanges();
                            return RedirectToAction("MaintainSamples");
                        }
                    }
                    model.ShowMessageCode = false;
                    model.ShowMessageDescription = false;
                    model.ValidationMessage = "";
                    model.IsNew = true;

                    return View("EditSamples", model);
                }
                catch (DbEntityValidationException ex)
                {
                    StringBuilder sb = new StringBuilder();

                    foreach (var failure in ex.EntityValidationErrors)
                    {
                        sb.AppendFormat("{0} failed validation:\n\n", failure.Entry.Entity.GetType());
                        foreach (var error in failure.ValidationErrors)
                        {
                            sb.AppendFormat("- {0} : {1}", error.PropertyName, error.ErrorMessage);
                            sb.AppendLine();
                        }
                    }
                    if (ex.InnerException == null)
                    {
                        ViewBag.Message = "Function: AdminController.EditSamplesSubmitSelected_POST\n\nError: " + ex.Message + "\n\n" + sb;
                    }
                    else
                    {
                        ViewBag.Message = "Function: AdminController.EditSamplesSubmitSelected_POST\n\nError: " + ex.Message + "\n\n" + sb + "\n\n" + ex.InnerException.Message;
                    };
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    saveFailed = true;

                    var entry = ex.Entries.Single();
                    entry.OriginalValues.SetValues(entry.GetDatabaseValues());
                }
                catch (DataException ex)
                {
                    if (ex.InnerException == null)
                    {
                        ViewBag.Message = "Function: AdminController.EditSamplesSubmitSelected_POST\n\nError: " + ex.Message + "\n\nBaseException: " + ex.GetBaseException().Message;
                    }
                    else
                    {
                        ViewBag.Message = "Function: AdminController.EditSamplesSubmitSelected_POST\n\nError: " + ex.Message + "\n\nBaseException: " + ex.GetBaseException().Message + "\n\nInnerException: " + ex.InnerException.Message;
                    };
                }
                catch (Exception ex)
                {
                    if (ex.InnerException == null)
                    {
                        ViewBag.Message = "Function: AdminController.EditSamplesSubmitSelected_POST\n\nError: " + ex.Message;
                    }
                    else
                    {
                        ViewBag.Message = "Function: AdminController.EditSamplesSubmitSelected_POST\n\nError: " + ex.Message + "\n\nBaseException: " + ex.InnerException.Message;
                    };
                };
            } while (saveFailed);

            Session["ErrorMessage"] = ViewBag.Message;
            return RedirectToAction("InternalServerError", "Error");
        }

        [MacAuthorize(Action = "AdministerAll,AdministerOwn")]
        [HttpGet]
        public ActionResult DeleteSamples(DeleteSamplesViewModel model)
        {
            try
            {
                return View(model);
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: AdminController.DeleteSamples_GET\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: AdminController.DeleteSamples_GET\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        [MacAuthorize(Action = "AdministerAll,AdministerOwn")]
        [HttpPost]
        public ActionResult DeleteSample(DeleteSamplesViewModel model)
        {
            try
            {
                int id = Convert.ToInt32(model.Id);
                if (uow.Repository<TRN_SAMPLE_TB>().Find(u => u.N_SAMPLE_TYPE_SYSID == id).Count() > 0)
                {
                    model.ShowMessage = true;
                    model.Message = "Service Log records exist for this sample. Sorry, the sample can not be deleted.";

                    return RedirectToAction("DeleteSamples", model);
                }
                else
                {
                    uow.Repository<REF_SAMPLE_TYPE_TB>().Delete(id);
                    uow.SaveChanges();

                    return RedirectToAction("MaintainSamples");
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: AdminController.DeleteSample_GET\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: AdminController.DeleteSample_GET\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        #endregion

        #region - Subprograms -

        [MacAuthorize(Action = "AdministerAll,AdministerOwn")]
        [HttpGet]
        public ActionResult MaintainSubprogram(MaintainSubprogramsViewModel Model)
        {
            try
            {
                return View(Model);
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: AdminController.MaintainSubprogram_GET\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: AdminController.MaintainSubprogram_GET\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        [MacAuthorize(Action = "AdministerAll,AdministerOwn")]
        [HttpGet]
        public ActionResult MaintainSubprogramsHelper(int ID)
        {
            try
            {
                REF_PROGRAM_TB program = null;
                if (ID > 0)
                {
                    program = uow.Repository<REF_PROGRAM_TB>().GetById(ID);
                    var model = new MaintainSubprogramsViewModel();
                    List<REF_SUBPROGRAM_TB> subprograms = uow.Repository<REF_SUBPROGRAM_TB>().GetAll().ToList();
                    List<REF_SUBPROGRAM_TB> subprogramsFiltered = subprograms.Where(s => s.N_PROGRAM_SYSID == program.N_PROGRAM_SYSID && s.SZ_CODE != "0").ToList<REF_SUBPROGRAM_TB>();
                    foreach (var subprogram in subprogramsFiltered.OrderBy(c => Int32.Parse(c.SZ_CODE)))
                    {
                        var editorVM = new MaintainSubprogramsEditorViewModel()
                        {
                            Id = subprogram.N_SUBPROGRAM_SYSID,
                            Code = subprogram.SZ_CODE,
                            Description = subprogram.SZ_DESCRIPTION,
                            ProgramCode = program.SZ_CODE,
                            ProgramDescription = program.SZ_DESCRIPTION,
                            Selected = false
                        };
                        model.Subprogram.Add(editorVM);
                    }
                    model.SelectedProgram = ID;
                    model.ProgramCode = program.SZ_CODE;
                    model.ProgramDescription = program.SZ_DESCRIPTION;
                    return View("MaintainSubprogram", model);
                }
                return RedirectToAction("EditPrograms");
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: AdminController.MaintainSubprogramsHelper_GET\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: AdminController.MaintainSubprogramsHelper_GET\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        [MacAuthorize(Action = "AdministerAll,AdministerOwn")]
        [HttpPost]
        public ActionResult MaintainSubprogramsSubmitSelected(MaintainSubprogramsViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    IEnumerable<int> selectedIds = model.getSelectedIds();
                    int id = selectedIds.FirstOrDefault();
                    string str = Request.Params["btnSubmit"];
                    if (id > 0)
                    {
                        REF_SUBPROGRAM_TB subprogram = uow.Repository<REF_SUBPROGRAM_TB>().GetById(id);

                        if (str == "Edit")
                        {
                            var viewModel = new EditSubprogramsViewModel()
                            {
                                Code = Convert.ToInt32(subprogram.SZ_CODE),
                                Description = subprogram.SZ_DESCRIPTION,
                                Id = subprogram.N_SUBPROGRAM_SYSID,
                                SelectedProgram = subprogram.N_PROGRAM_SYSID,
                                IsNew = false,
                                ShowMessageCode = false,
                                ShowMessageDescription = false,
                                ValidationMessage = ""
                            };
                            return View("EditSubprogram", viewModel);
                        }
                        else //str == "Delete"
                        {
                            var deleteSubprogramsModel = new DeleteSubprogramsViewModel()
                            {
                                Id = subprogram.N_SUBPROGRAM_SYSID,
                                Code = Convert.ToInt32(subprogram.SZ_CODE),
                                Description = subprogram.SZ_DESCRIPTION,
                                SelectedProgram = subprogram.N_PROGRAM_SYSID,
                                Message = "",
                                ShowMessage = false
                            };
                            return RedirectToAction("DeleteSubprograms", deleteSubprogramsModel);
                        }
                    }
                    else
                    {
                        if (str == "Add")
                        {
                            var viewModel = new EditSubprogramsViewModel()
                            {
                                SelectedProgram = model.SelectedProgram,
                                Code = subprogramRepo.GetNextSubprogramCode(),
                                Description = "",
                                Id = 0,
                                IsNew = true,
                                ShowMessageCode = false,
                                ShowMessageDescription = false,
                                ValidationMessage = ""
                            };
                            return View("EditSubprogram", viewModel);
                        }
                        else // str=="Edit"
                        {
                            REF_PROGRAM_TB program = uow.Repository<REF_PROGRAM_TB>().GetById(model.SelectedProgram);
                            var myModel = new MaintainSubprogramsViewModel();
                            List<REF_SUBPROGRAM_TB> subprograms = uow.Repository<REF_SUBPROGRAM_TB>().GetAll().ToList();
                            List<REF_SUBPROGRAM_TB> subprogramsFiltered = subprograms.Where(s => s.N_PROGRAM_SYSID == program.N_PROGRAM_SYSID && s.SZ_CODE != "0").ToList<REF_SUBPROGRAM_TB>();
                            foreach (var subprogram in subprogramsFiltered.OrderBy(c => Int32.Parse(c.SZ_CODE)))
                            {
                                var editorVM = new MaintainSubprogramsEditorViewModel()
                                {
                                    Id = subprogram.N_SUBPROGRAM_SYSID,
                                    Code = subprogram.SZ_CODE,
                                    Description = subprogram.SZ_DESCRIPTION,
                                    ProgramCode = program.SZ_CODE,
                                    ProgramDescription = program.SZ_DESCRIPTION,
                                    Selected = false
                                };
                                myModel.Subprogram.Add(editorVM);
                            }
                            myModel.ShowEditMessage = true;
                            myModel.EditMessage = "Please select a subprogram";
                            myModel.ProgramCode = subprogramsFiltered.FirstOrDefault().REF_PROGRAM_TB.SZ_CODE;
                            myModel.ProgramDescription = subprogramsFiltered.FirstOrDefault().REF_PROGRAM_TB.SZ_DESCRIPTION;

                            return View("MaintainSubprogram", myModel);
                        }
                    }
                }
                return View("MaintainSubprogram", model);
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: AdminController.MaintainSubprogramsSubmitSelected_POST\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: AdminController.MaintainSubprogramsSubmitSelected_POST\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        [MacAuthorize(Action = "AdministerAll,AdministerOwn")]
        [HttpGet]
        public ActionResult EditSubprogram(EditSubprogramsViewModel model)
        {
            try
            {
                return View(model);
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: AdminController.EditSubprogram_GET\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: AdminController.EditSubprogram_GET\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };

        }

        [MacAuthorize(Action = "AdministerAll,AdministerOwn")]
        [HttpPost]
        public ActionResult EditSubprogramSubmit(EditSubprogramsViewModel model)
        {
            bool saveFailed;
            do
            {
                saveFailed = false;
                try
                {
                    if (ModelState.IsValid)
                    {
                        //bool isSubprogramNumberDuplicate = false;
                        bool isSubprogramDescriptionDuplicate = false;

                        //if (model.Id == 0)
                        //{
                        //    isSubprogramNumberDuplicate = subprogramRepo.IsSubprogramNumberDuplicate(model.Code, model.SelectedProgram);
                        //}

                        isSubprogramDescriptionDuplicate = subprogramRepo.IsSubprogramDescriptionDuplicate(model.Description, model.SelectedProgram);

                        //if (isSubprogramNumberDuplicate)
                        //{
                        //    model.IsNew = true;
                        //    model.ShowMessageCode = true;
                        //    model.ValidationMessage = "The subprogram number already exists. Please enter different one.";
                        //
                        //    return View("EditSubprogram", model);
                        //}
                        if (isSubprogramDescriptionDuplicate)
                        {
                            model.ShowMessageDescription = true;
                            model.ValidationMessage = "The subprogram description already exists. Please enter different one.";

                            return View("EditSubprogram", model);
                        }
                        else
                        {
                            if (model.Id == 0)
                            {
                                REF_SUBPROGRAM_TB subprogram = new REF_SUBPROGRAM_TB()
                                {
                                    B_INACTIVE = false,
                                    DT_MODIFIED = DateTime.UtcNow,
                                    SZ_CODE = model.Code.ToString(),
                                    SZ_DESCRIPTION = model.Description,
                                    N_PROGRAM_SYSID = model.SelectedProgram,
                                    SZ_MODIFIED_BY = modifiedBy
                                };
                                uow.Repository<REF_SUBPROGRAM_TB>().Add(subprogram);
                            }
                            else
                            {
                                REF_SUBPROGRAM_TB subprogram = uow.Repository<REF_SUBPROGRAM_TB>().GetById(model.Id);
                                subprogram.DT_MODIFIED = DateTime.UtcNow;
                                subprogram.SZ_CODE = model.Code.ToString();
                                subprogram.SZ_DESCRIPTION = model.Description;
                                subprogram.SZ_MODIFIED_BY = modifiedBy;
                                uow.Repository<REF_SUBPROGRAM_TB>().Update(subprogram);
                            }
                            uow.SaveChanges();
                            return RedirectToAction("MaintainSubprogramsHelper", new { ID = model.SelectedProgram });
                        }
                    }
                    model.ShowMessageCode = false;
                    model.ShowMessageDescription = false;
                    model.ValidationMessage = "";
                    model.IsNew = true;

                    return View("EditSubprogram", model);
                }
                catch (DbEntityValidationException ex)
                {
                    StringBuilder sb = new StringBuilder();

                    foreach (var failure in ex.EntityValidationErrors)
                    {
                        sb.AppendFormat("{0} failed validation:\n\n", failure.Entry.Entity.GetType());
                        foreach (var error in failure.ValidationErrors)
                        {
                            sb.AppendFormat("- {0} : {1}", error.PropertyName, error.ErrorMessage);
                            sb.AppendLine();
                        }
                    }
                    if (ex.InnerException == null)
                    {
                        ViewBag.Message = "Function: AdminController.EditSubprogramSubmit_POST\n\nError: " + ex.Message + "\n\n" + sb;
                    }
                    else
                    {
                        ViewBag.Message = "Function: AdminController.EditSubprogramSubmit_POST\n\nError: " + ex.Message + "\n\n" + sb + "\n\n" + ex.InnerException.Message;
                    };
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    saveFailed = true;

                    var entry = ex.Entries.Single();
                    entry.OriginalValues.SetValues(entry.GetDatabaseValues());
                }
                catch (DataException ex)
                {
                    if (ex.InnerException == null)
                    {
                        ViewBag.Message = "Function: AdminController.EditSubprogramSubmit_POST\n\nError: " + ex.Message + "\n\nBaseException: " + ex.GetBaseException().Message;
                    }
                    else
                    {
                        ViewBag.Message = "Function: AdminController.EditSubprogramSubmit_POST\n\nError: " + ex.Message + "\n\nBaseException: " + ex.GetBaseException().Message + "\n\nInnerException: " + ex.InnerException.Message;
                    };
                }
                catch (Exception ex)
                {
                    if (ex.InnerException == null)
                    {
                        ViewBag.Message = "Function: AdminController.EditSubprogramSubmit_POST\n\nError: " + ex.Message;
                    }
                    else
                    {
                        ViewBag.Message = "Function: AdminController.EditSubprogramSubmit_POST\n\nError: " + ex.Message + "\n\nBaseException: " + ex.InnerException.Message;
                    };
                };
            } while (saveFailed);

            Session["ErrorMessage"] = ViewBag.Message;
            return RedirectToAction("InternalServerError", "Error");
        }

        [MacAuthorize(Action = "AdministerAll,AdministerOwn")]
        [HttpGet]
        public ActionResult DeleteSubprograms(DeleteSubprogramsViewModel model)
        {
            try
            {
                return View(model);
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: AdminController.DeleteSubprograms_GET\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: AdminController.DeleteSubprograms_GET\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        [MacAuthorize(Action = "AdministerAll,AdministerOwn")]
        [HttpPost]
        public ActionResult DeleteSubprogram(DeleteSubprogramsViewModel model)
        {
            try
            {
                int id = Convert.ToInt32(model.Id);
                if (uow.Repository<TRN_SERVICE_X_ACTIVITY_TYPE_TB>().Find(u => u.N_SUBPROGRAM_SYSID == id).Count() > 0)
                {
                    model.ShowMessage = true;
                    model.Message = "Service Log records exist for this subprogram. Sorry, the subprogram can not be deleted.";

                    return RedirectToAction("DeleteSubprograms", model);
                }
                else
                {
                    uow.Repository<REF_SUBPROGRAM_TB>().Delete(id);
                    uow.SaveChanges();

                    return RedirectToAction("MaintainSubprogramsHelper", new { ID = model.SelectedProgram });
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: AdminController.DeleteSubprogram_GET\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: AdminController.DeleteSubprogram_GET\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        #endregion

        #region - Workers -

        [MacAuthorize(Action = "AdministerAll,AdministerOwn")]
        [HttpGet]
        public ActionResult MaintainWorkers(bool DisplayEditMessage = false)
        {
            try
            {
                var model = new MaintainWorkersViewModel();
                if (DisplayEditMessage == true)
                {
                    model.ShowEditMessage = true;
                    model.EditMessage = "Please select an activity";
                }
                List<REF_WORKER_TB> workers = uow.Repository<REF_WORKER_TB>().GetAll().ToList();
                foreach (var worker in workers.OrderBy(c => c.SZ_USER_NAME))
                {
                    var editorVM = new MaintainWorkersEditorViewModel()
                    {
                        Id = worker.N_WORKER_SYSID,
                        UserName = worker.SZ_USER_NAME,
                        WorkerNumber = worker.N_WORKER_NUMBER.ToString(),
                        Selected = false
                    };
                    model.Worker.Add(editorVM);
                }
                return View(model);
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: AdminController.MaintainWorkers_GET\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: AdminController.MaintainWorkers_GET\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        [MacAuthorize(Action = "AdministerAll,AdministerOwn")]
        [HttpPost]
        public ActionResult MaintainWorkersSubmitSelected(MaintainWorkersViewModel model, string Action)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    IEnumerable<int> selectedIds = model.getSelectedIds();
                    int id = selectedIds.FirstOrDefault();
                    if (id != null)
                    {
                        REF_WORKER_TB worker = db.REF_WORKER_TB.Where(h => h.N_WORKER_SYSID == id).FirstOrDefault();

                        if (Action == "Edit")
                        {
                            var editWorkersModel = new EditWorkersViewModel()
                            {
                                Id = worker.N_WORKER_SYSID,
                                UserName = worker.SZ_USER_NAME,
                                WorkerNumber = worker.N_WORKER_NUMBER.ToString(),
                                IsNew = false,
                                ShowMessageCode = false,
                                ShowMessageDescription = false,
                                ValidationMessage = ""
                            };
                            return View("EditWorkers", editWorkersModel);
                        }
                        else //Action == "Delete"
                        {
                            var deleteWorkersModel = new DeleteWorkersViewModel()
                            {
                                Id = worker.N_WORKER_SYSID,
                                UserName = worker.SZ_USER_NAME,
                                WorkerNumber = worker.N_WORKER_NUMBER.ToString(),
                                Message = "",
                                ShowMessage = false
                            };
                            return View("DeleteWorkers", deleteWorkersModel);
                        }
                    }
                    else
                    {
                        return RedirectToAction("MaintainWorkers", new { DisplayEditMessage = true });
                    }
                }

                return View("MaintainWorkers");
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: AdminController.MaintainWorkersSubmitSelected_POST\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: AdminController.MaintainWorkersSubmitSelected_POST\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        [MacAuthorize(Action = "AdministerAll,AdministerOwn")]
        [HttpGet]
        public ActionResult EditWorkers(EditWorkersViewModel model)
        {
            try
            {
                return View(model);
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: AdminController.EditWorkers_GET\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: AdminController.EditWorkers_GET\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        [MacAuthorize(Action = "AdministerAll,AdministerOwn")]
        [HttpGet]
        public ActionResult DeleteWorkers(DeleteWorkersViewModel model)
        {
            try
            {
                return View(model);
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: AdminController.DeleteWorkers_GET\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: AdminController.DeleteWorkers_GET\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        [MacAuthorize(Action = "AdministerAll,AdministerOwn")]
        [HttpPost]
        public ActionResult DeleteWorker(DeleteWorkersViewModel model)
        {
            try
            {
                int id = model.Id;
                int workerNumber = Convert.ToInt32(model.WorkerNumber);
                if (uow.Repository<TRN_SERVICE_TB>().Find(u => u.N_WORKER_NUMBER == workerNumber).Count() > 0)
                {
                    model.ShowMessage = true;
                    model.Message = "Service Log records exist for this worker number. Sorry, the worker can not be deleted.";

                    return RedirectToAction("DeleteWorkers", model);
                }
                else
                {
                    REF_WORKER_TB worker = db.REF_WORKER_TB.Where(h => h.N_WORKER_SYSID == id).FirstOrDefault();
                    db.REF_WORKER_TB.Remove(worker);
                    db.SaveChanges();

                    return RedirectToAction("MaintainWorkers");
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: AdminController.DeleteWorker_GET\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: AdminController.DeleteWorker_GET\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        [MacAuthorize(Action = "AdministerAll,AdministerOwn")]
        [HttpPost]
        public ActionResult InsertWorkersSubmitSelected()
        {
            try
            {
                var editWorkersModel = new EditWorkersViewModel()
                {
                    Id = 0,
                    UserName = "",
                    WorkerNumber = workerRepo.GetNextWorkerNumber().ToString(),
                    IsNew = true,
                    ShowMessageCode = false,
                    ShowMessageDescription = false,
                    ValidationMessage = ""
                };
                return View("EditWorkers", editWorkersModel);
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: AdminController.InsertWorkersSubmitSelected_POST\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: AdminController.InsertWorkersSubmitSelected_POST\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        [MacAuthorize(Action = "AdministerAll,AdministerOwn")]
        [HttpPost]
        public ActionResult EditWorkersSubmitSelected(EditWorkersViewModel model)
        {
            bool saveFailed;
            do
            {
                saveFailed = false;
                try
                {
                    if (ModelState["WorkerNumber"].Errors.Count > 0)
                    {
                        string code = ModelState["WorkerNumber"].Value.AttemptedValue.ToString();
                        string[] parts = code.Split(',');
                        string entry = parts[0];
                        string response = "The value: '" + entry + "' is not valid. Please enter a numeric value.";
                        ModelState.Remove("WorkerNumber");
                        ModelState.AddModelError("WorkerNumber", response);
                    }

                    if (ModelState.IsValid)
                    {
                        bool isWorkerNumberDuplicate = false;

                        isWorkerNumberDuplicate = workerRepo.IsWorkerNumberDuplicate(Convert.ToInt32(model.WorkerNumber));

                        if (isWorkerNumberDuplicate && model.Id == 0)
                        {
                            model.ShowMessageDescription = true;
                            model.ValidationMessage = "The worker number already exists. Please enter a different one.";

                            return View("EditWorkers", model);
                        }
                        else
                        {
                            string modifiedBy = null;
                            if (SessionHelper.UserName == null)
                            {
                                modifiedBy = "Test";
                            }
                            else
                            {
                                modifiedBy = SessionHelper.UserName;
                            }
                            if (model.Id == 0)
                            {
                                REF_WORKER_TB worker = new REF_WORKER_TB()
                                {
                                    N_WORKER_SYSID = workerRepo.GetNextWorkerID(),
                                    B_INACTIVE = false,
                                    DT_MODIFIED = DateTime.UtcNow,
                                    SZ_USER_NAME = model.UserName,
                                    N_WORKER_NUMBER = Convert.ToInt32(model.WorkerNumber),
                                    SZ_MODIFIED_BY = modifiedBy
                                };
                                uow.Repository<REF_WORKER_TB>().Add(worker);
                            }
                            else
                            {
                                REF_WORKER_TB worker = db.REF_WORKER_TB.Where(h => h.N_WORKER_SYSID == model.Id).FirstOrDefault();
                                worker.DT_MODIFIED = DateTime.UtcNow;
                                worker.SZ_USER_NAME = model.UserName;
                                worker.N_WORKER_NUMBER = Convert.ToInt32(model.WorkerNumber);
                                worker.SZ_MODIFIED_BY = modifiedBy;
                                uow.Repository<REF_WORKER_TB>().Update(worker);
                            }
                            uow.SaveChanges();
                            return RedirectToAction("MaintainWorkers");
                        }
                    }
                    model.ShowMessageCode = false;
                    model.ShowMessageDescription = false;
                    model.ValidationMessage = "";
                    model.IsNew = true;

                    return View("EditWorkers", model);
                }
                catch (DbEntityValidationException ex)
                {
                    StringBuilder sb = new StringBuilder();

                    foreach (var failure in ex.EntityValidationErrors)
                    {
                        sb.AppendFormat("{0} failed validation:\n\n", failure.Entry.Entity.GetType());
                        foreach (var error in failure.ValidationErrors)
                        {
                            sb.AppendFormat("- {0} : {1}", error.PropertyName, error.ErrorMessage);
                            sb.AppendLine();
                        }
                    }
                    if (ex.InnerException == null)
                    {
                        ViewBag.Message = "Function: AdminController.EditWorkersSubmitSelected_POST\n\nError: " + ex.Message + "\n\n" + sb;
                    }
                    else
                    {
                        ViewBag.Message = "Function: AdminController.EditWorkersSubmitSelected_POST\n\nError: " + ex.Message + "\n\n" + sb + "\n\n" + ex.InnerException.Message;
                    };
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    saveFailed = true;

                    var entry = ex.Entries.Single();
                    entry.OriginalValues.SetValues(entry.GetDatabaseValues());
                }
                catch (DataException ex)
                {
                    if (ex.InnerException == null)
                    {
                        ViewBag.Message = "Function: AdminController.EditWorkersSubmitSelected_POST\n\nError: " + ex.Message + "\n\nBaseException: " + ex.GetBaseException().Message;
                    }
                    else
                    {
                        ViewBag.Message = "Function: AdminController.EditWorkersSubmitSelected_POST\n\nError: " + ex.Message + "\n\nBaseException: " + ex.GetBaseException().Message + "\n\nInnerException: " + ex.InnerException.Message;
                    };
                }
                catch (Exception ex)
                {
                    if (ex.InnerException == null)
                    {
                        ViewBag.Message = "Function: AdminController.EditWorkersSubmitSelected_POST\n\nError: " + ex.Message;
                    }
                    else
                    {
                        ViewBag.Message = "Function: AdminController.EditWorkersSubmitSelected_POST\n\nError: " + ex.Message + "\n\nBaseException: " + ex.InnerException.Message;
                    };
                };
            } while (saveFailed);

            Session["ErrorMessage"] = ViewBag.Message;
            return RedirectToAction("InternalServerError", "Error");
        }

        #endregion
    }
}