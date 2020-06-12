using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.AspNet.Identity;
using DotNetOpenAuth.OAuth2;
using EH.TimeTrackNet.Web.Filters;
using EH.TimeTrackNet.Web.Models;
using EH.TimeTrackNet.Web.Repositories;
using EH.TimeTrackNet.Web.Utilities;
using EH.TimeTrackNet.Web.ViewModels;

namespace EH.TimeTrackNet.Web.Controllers
{
    [MacAuthorize(Action = "ViewPublished")]
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    public class UserController : BaseController
    {
        private GenericEHTimetrackUnitOfWork _uow = null;
        private ActivityGet _activityRepo = null;
        private CivilDivisionGet _civilDivisionRepo = null;
        private DailyActivityGet _dailyActivityRepo = null;
        private OptionGet _optionRepo = null;
        private ProgramGet _programRepo = null;
        private SampleGet _sampleRepo = null;
        private ServiceLogGet _serviceLogRepo = null;
        private SubprogramGet _subprogramRepo = null;
        private WorkerGet _workerRepo = null;
        private string _modifiedBy = null;
        //private WebServerClient _webServerClient;
        private HttpClient _client;
        //private Uri _resourceServerUri = new Uri(ConfigurationManager.AppSettings["OAuth.MacombCounty.Paths.Remote.ResourceServerBaseAddress"]);
        
        public UserController()
        {
            _uow = new GenericEHTimetrackUnitOfWork();
            _activityRepo = new ActivityGet();
            _civilDivisionRepo = new CivilDivisionGet();
            _dailyActivityRepo = new DailyActivityGet();
            _optionRepo = new OptionGet();
            _programRepo = new ProgramGet();
            _sampleRepo = new SampleGet();
            _serviceLogRepo = new ServiceLogGet();
            _subprogramRepo = new SubprogramGet();
            _workerRepo = new WorkerGet();

            if (SessionHelper.UserName == null)
            {
                _modifiedBy = "Test";
            }
            else
            {
                _modifiedBy = SessionHelper.UserName;
            }
        }

        #region - Service Log -

        [HttpGet]
        [OutputCache(Duration = 0)]
        public ActionResult ServiceLog()
        {
            try
            {
                List<SelectListItem> workerList = null;
                List<User> userList = GetUsersByDepartment("{Department:'" + ConfigurationManager.AppSettings["Application.Department"] +
                    "',Description:'" + ConfigurationManager.AppSettings["Application.Description"] + "'}");

                if (User.HasAction("AdministerAll"))
                {
                    workerList = (List<SelectListItem>)userList
                                .Select(u => new SelectListItem { Value = u.PrincipalName, Text = (u.DisplayName + "  /  " + GetUserProperty(u.PrincipalName, "SAMAccountName")) })
                                .OrderBy(u => u.Text)
                                .ToList();
                }
                else
                {
                    workerList = (List<SelectListItem>)userList
                        .Where(u => u.PrincipalName == User.Identity.Name)
                                .Select(u => new SelectListItem { Value = u.PrincipalName, Text = (u.DisplayName + "  /  " + GetUserProperty(u.PrincipalName, "SAMAccountName")) })
                                .OrderBy(u => u.Text)
                                .ToList();
                }

                // workaround for testing - add current user to dropdown
                //if (!workerList.Select(u => u.Text).Contains(SessionHelper.DisplayName + " / " + GetUserProperty(SessionHelper.UserName, "SAMAccountName")))
                //{
                //    var item = new SelectListItem { Value = SessionHelper.UserName, Text = (SessionHelper.DisplayName + " / " + GetUserProperty(SessionHelper.UserName, "SAMAccountName")) };
                //    workerList.Add(item);
                //    workerList = workerList.OrderBy(u => u.Value).Distinct().ToList();
                //}

                var model = new ServiceLogViewModel()
                {
                    Workers = workerList,
                    ServiceDate = DateTime.UtcNow.ToShortDateString(),
                    ShowDateMessage = false,
                    DateValidationMessage = ""
                };
                return View(model);
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: UserController.ServiceLog_GET\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: UserController.ServiceLog_GET\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        [HttpGet]
        public ActionResult RenderServiceLog(string ServiceLogID, string SelectedActivity = "None", string SelectedSample = "None")
        {
            try
            {
                // renders daily activity log view
                TRN_SERVICE_TB log = _serviceLogRepo.GetServiceLog(Convert.ToInt32(ServiceLogID));
                decimal totalActivityTime = Convert.ToDecimal(_activityRepo.GetTotalActivityTime(log.N_SERVICE_SYSID));
                decimal totalTime = Convert.ToDecimal(_activityRepo.GetTotalActivityTime(log.N_SERVICE_SYSID) + log.N_MILEAGE_TIME);
                DailyActivityLogViewModel viewModel = new DailyActivityLogViewModel()
                {
                    SelectedActivity = SelectedActivity,
                    SelectedSample = SelectedSample,
                    ServiceLogID = log.N_SERVICE_SYSID,
                    MileageTime = log.N_MILEAGE_TIME,
                    TotalActivityTime = totalActivityTime,
                    TotalTime = totalTime,
                    ServiceDate = log.DT_SERVICE.ToShortDateString(),
                    WorkerName = GetUserProperty(log.SZ_USER_NAME, "DisplayName"),
                    IsValid = true
                };
                return RedirectToAction("DailyActivityLog", viewModel);
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: UserController.RenderServiceLog_GET\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: UserController.RenderServiceLog_GET\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        [HttpGet]
        public ActionResult CancelServiceLog(string ServiceLogID)
        {
            bool saveFailed;
            do
            {
                saveFailed = false;
                try
                {
                    IEnumerable<TRN_SERVICE_X_ACTIVITY_TYPE_TB> activities = _activityRepo.GetActivitiesByServiceLogID(Convert.ToInt32(ServiceLogID));
                    foreach (TRN_SERVICE_X_ACTIVITY_TYPE_TB activity in activities)
                    {
                        IEnumerable<TRN_SAMPLE_TB> samples = _sampleRepo.GetSamplesByActivityID(activity.N_SERVICE_X_ACTIVITY_TYPE_SYSID);
                        foreach (TRN_SAMPLE_TB sample in samples)
                        {
                            _uow.Repository<TRN_SAMPLE_TB>().Delete(sample.N_SAMPLE_SYSID);
                        }
                        _uow.Repository<TRN_SERVICE_X_ACTIVITY_TYPE_TB>().Delete(activity.N_SERVICE_X_ACTIVITY_TYPE_SYSID);
                    }
                    _uow.Repository<TRN_SERVICE_TB>().Delete(Convert.ToInt32(ServiceLogID));
                    _uow.SaveChanges();
                    return RedirectToAction("ServiceLog");
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
                        ViewBag.Message = "Function: UserController.CancelServiceLog_GET\n\nError: " + ex.Message + "\n\n" + sb;
                    }
                    else
                    {
                        ViewBag.Message = "Function: UserController.CancelServiceLog_GET\n\nError: " + ex.Message + "\n\n" + sb + "\n\n" + ex.InnerException.Message;
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
                        ViewBag.Message = "Function: UserController.CancelServiceLog_GET\n\nError: " + ex.Message + "\n\nBaseException: " + ex.GetBaseException().Message;
                    }
                    else
                    {
                        ViewBag.Message = "Function: UserController.CancelServiceLog_GET\n\nError: " + ex.Message + "\n\nBaseException: " + ex.GetBaseException().Message + "\n\nInnerException: " + ex.InnerException.Message;
                    };
                }
                catch (Exception ex)
                {
                    if (ex.InnerException == null)
                    {
                        ViewBag.Message = "Function: UserController.CancelServiceLog_GET\n\nError: " + ex.Message;
                    }
                    else
                    {
                        ViewBag.Message = "Function: UserController.CancelServiceLog_GET\n\nError: " + ex.Message + "\n\nBaseException: " + ex.InnerException.Message;
                    };
                };
            } while (saveFailed);

            Session["ErrorMessage"] = ViewBag.Message;
            return RedirectToAction("InternalServerError", "Error");
        }

        [HttpGet]
        public ActionResult DeleteServiceLog(string ServiceLogID)
        {
            bool saveFailed;
            do
            {
                saveFailed = false;
                try
                {
                    IEnumerable<TRN_SERVICE_X_ACTIVITY_TYPE_TB> activities = _activityRepo.GetActivitiesByServiceLogID(Convert.ToInt32(ServiceLogID));
                    foreach (TRN_SERVICE_X_ACTIVITY_TYPE_TB activity in activities)
                    {
                        IEnumerable<TRN_SAMPLE_TB> samples = _sampleRepo.GetSamplesByActivityID(activity.N_SERVICE_X_ACTIVITY_TYPE_SYSID);
                        foreach (TRN_SAMPLE_TB sample in samples)
                        {
                            _uow.Repository<TRN_SAMPLE_TB>().Delete(sample.N_SAMPLE_SYSID);
                        }
                        _uow.Repository<TRN_SERVICE_X_ACTIVITY_TYPE_TB>().Delete(activity.N_SERVICE_X_ACTIVITY_TYPE_SYSID);
                    }
                    _uow.Repository<TRN_SERVICE_TB>().Delete(Convert.ToInt32(ServiceLogID));
                    _uow.SaveChanges();
                    return RedirectToAction("RenderDailySummaryLog");
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
                        ViewBag.Message = "Function: UserController.DeleteServiceLog_GET\n\nError: " + ex.Message + "\n\n" + sb;
                    }
                    else
                    {
                        ViewBag.Message = "Function: UserController.DeleteServiceLog_GET\n\nError: " + ex.Message + "\n\n" + sb + "\n\n" + ex.InnerException.Message;
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
                        ViewBag.Message = "Function: UserController.DeleteServiceLog_GET\n\nError: " + ex.Message + "\n\nBaseException: " + ex.GetBaseException().Message;
                    }
                    else
                    {
                        ViewBag.Message = "Function: UserController.DeleteServiceLog_GET\n\nError: " + ex.Message + "\n\nBaseException: " + ex.GetBaseException().Message + "\n\nInnerException: " + ex.InnerException.Message;
                    };
                }
                catch (Exception ex)
                {
                    if (ex.InnerException == null)
                    {
                        ViewBag.Message = "Function: UserController.DeleteServiceLog_GET\n\nError: " + ex.Message;
                    }
                    else
                    {
                        ViewBag.Message = "Function: UserController.DeleteServiceLog_GET\n\nError: " + ex.Message + "\n\nBaseException: " + ex.InnerException.Message;
                    };
                };
            } while (saveFailed);

            Session["ErrorMessage"] = ViewBag.Message;
            return RedirectToAction("InternalServerError", "Error");
        }

        [HttpPost]
        public ActionResult ServiceLog(ServiceLogViewModel model)
        {
            DateTime dateTime;
            if (ModelState.IsValid && DateTime.TryParse(model.ServiceDate, out dateTime))
            {
                bool saveFailed;
                do
                {
                    saveFailed = false;
                    try
                    {
                        string workerName = model.SelectedWorker.ToString();
                        TRN_SERVICE_TB serviceLog = null;
                        TRN_SERVICE_TB log = _serviceLogRepo.GetServiceLog(Convert.ToDateTime(model.ServiceDate), model.SelectedWorker);
                        if (log == null)
                        {
                            TRN_SERVICE_TB LogNew = new TRN_SERVICE_TB()
                            {
                                B_INACTIVE = false,
                                DT_MODIFIED = DateTime.UtcNow,
                                DT_SERVICE = Convert.ToDateTime(model.ServiceDate),
                                SZ_MODIFIED_BY = _modifiedBy,
                                SZ_USER_NAME = workerName,
                                N_WORKER_NUMBER = _workerRepo.GetWorkerNumber(workerName)
                            };
                            _uow.Repository<TRN_SERVICE_TB>().Add(LogNew);
                            _uow.SaveChanges();
                            serviceLog = _serviceLogRepo.GetServiceLog(Convert.ToDateTime(model.ServiceDate), model.SelectedWorker);
                        }
                        else
                        {
                            log.DT_MODIFIED = DateTime.UtcNow;
                            log.DT_SERVICE = Convert.ToDateTime(model.ServiceDate);
                            log.SZ_MODIFIED_BY = _modifiedBy;
                            log.SZ_USER_NAME = workerName;
                            _uow.Repository<TRN_SERVICE_TB>().Update(log);
                            _uow.SaveChanges();
                            serviceLog = log;
                        }
                        int serviceLogID = 0;
                        if (serviceLog != null) serviceLogID = serviceLog.N_SERVICE_SYSID;
                        decimal totalActivityTime = Convert.ToDecimal(_activityRepo.GetTotalActivityTime(serviceLog.N_SERVICE_SYSID));
                        decimal totalTime = Convert.ToDecimal(_activityRepo.GetTotalActivityTime(serviceLog.N_SERVICE_SYSID) + serviceLog.N_MILEAGE_TIME);
                        DailyActivityLogViewModel viewModel = new DailyActivityLogViewModel()
                        {
                            SelectedActivity = "None",
                            SelectedSample = "None",
                            ServiceLogID = serviceLogID,
                            MileageTime = serviceLog.N_MILEAGE_TIME,
                            TotalActivityTime = totalActivityTime,
                            TotalTime = totalTime,
                            ServiceDate = serviceLog.DT_SERVICE.ToShortDateString(),
                            WorkerName = GetUserProperty(serviceLog.SZ_USER_NAME, "DisplayName"),
                            IsValid = true
                        };
                        return RedirectToAction("DailyActivityLog", viewModel);
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
                            ViewBag.Message = "Function: UserController.ServiceLog_Post\n\nError: " + ex.Message + "\n\n" + sb;
                        }
                        else
                        {
                            ViewBag.Message = "Function: UserController.ServiceLog_Post\n\nError: " + ex.Message + "\n\n" + sb + "\n\n" + ex.InnerException.Message;
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
                            ViewBag.Message = "Function: UserController.ServiceLog_Post\n\nError: " + ex.Message + "\n\nBaseException: " + ex.GetBaseException().Message;
                        }
                        else
                        {
                            ViewBag.Message = "Function: UserController.ServiceLog_Post\n\nError: " + ex.Message + "\n\nBaseException: " + ex.GetBaseException().Message + "\n\nInnerException: " + ex.InnerException.Message;
                        };
                    }
                    catch (Exception ex)
                    {
                        if (ex.InnerException == null)
                        {
                            ViewBag.Message = "Function: UserController.ServiceLog_Post\n\nError: " + ex.Message;
                        }
                        else
                        {
                            ViewBag.Message = "Function: UserController.ServiceLog_Post\n\nError: " + ex.Message + "\n\nBaseException: " + ex.InnerException.Message;
                        };
                    };
                } while (saveFailed);

                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            }
            
            try
            {
                List<User> userList = GetUsersByDepartment("{Department:'" + ConfigurationManager.AppSettings["Application.Department"] +
                    "',Description:'" + ConfigurationManager.AppSettings["Application.Description"] + "'}");

                List<SelectListItem> workerList;

                if (User.HasAction("AdministerAll"))
                {
                    workerList = (List<SelectListItem>)userList
                                .Select(u => new SelectListItem { Value = u.PrincipalName, Text = (u.DisplayName + "  /  " + GetUserProperty(u.PrincipalName, "SAMAccountName")) })
                                .OrderBy(u => u.Text)
                                .ToList();
                }
                else
                {
                    workerList = (List<SelectListItem>)userList
                                .Where(u => u.UserName == SessionHelper.UserName)
                                .Select(u => new SelectListItem { Value = u.PrincipalName, Text = (u.DisplayName + "  /  " + GetUserProperty(u.PrincipalName, "SAMAccountName")) })
                                .OrderBy(u => u.Text)
                                .ToList();
                }

                // workaround for testing - add current user to dropdown
                if (!workerList.Select(u => u.Text).Contains(SessionHelper.DisplayName + " / " + GetUserProperty(SessionHelper.UserName, "SAMAccountName")))
                {
                    var item = new SelectListItem { Value = SessionHelper.UserName, Text = (SessionHelper.DisplayName + " / " + GetUserProperty(SessionHelper.UserName, "SAMAccountName")) };
                    workerList.Add(item);
                    workerList = workerList.OrderBy(u => u.Value).ToList();
                }

                var newModel = new ServiceLogViewModel()
                {
                    Workers = workerList,
                    ServiceDate = model.ServiceDate,
                    SelectedWorker = model.SelectedWorker,
                    ShowDateMessage = !DateTime.TryParse(model.ServiceDate, out dateTime),
                    DateValidationMessage = "Please enter the Service Date in the correct format"
                };

                return View(newModel);
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: UserController.ServiceLog_Post\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: UserController.ServiceLog_Post\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        #endregion

        #region - Daily Activity Log -

        [HttpGet]
        [OutputCache(Duration = 0)]
        public ActionResult DailyActivityLog(DailyActivityLogViewModel model)
        {
            try
            {
                ViewBag.SelectedActivity = model.SelectedActivity;
                ViewBag.SelectedSample = model.SelectedSample;
                return View(model);
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: UserController.DailyActivityLog_GET\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: UserController.DailyActivityLog_GET\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        [HttpPost]
        public ActionResult PersistMileageTime(DailyActivityLogViewModel model)
        {
            decimal mileage;
            try
            {
                bool canParse = Decimal.TryParse(model.MileageTime.ToString(), out mileage);
                string mileageTime = ModelState["MileageTime"].Value.AttemptedValue.ToString();
                if (!(mileageTime == ""))
                {
                    if (!canParse || model.MileageTime.ToString().Length > 4)
                    {
                        string[] parts = mileageTime.Split(',');
                        string entry = parts[0];
                        string response = "The value: '" + entry + "' is not valid. Please enter a numeric value with no more than four characters.";
                        if (ModelState["MileageTime"].Errors.Count > 0) ModelState.Remove("MileageTime");
                        ModelState.AddModelError("MileageTime", response);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: UserController.PersistMileageTime_POST\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: UserController.PersistMileageTime_POST\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };

            if (ModelState.IsValid)
            {
                bool saveFailed;
                do
                {
                    saveFailed = false;
                    try
                    {
                        TRN_SERVICE_TB log = _serviceLogRepo.GetServiceLog(model.ServiceLogID);
                        log.DT_MODIFIED = DateTime.UtcNow;
                        log.SZ_MODIFIED_BY = _modifiedBy;
                        log.N_MILEAGE_TIME = mileage;
                        _uow.Repository<TRN_SERVICE_TB>().Update(log);
                        _uow.SaveChanges();
                        return RedirectToAction("RedirectDailyActivityLog", new { serviceLogID = model.ServiceLogID.ToString() });
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
                            ViewBag.Message = "Function: UserController.PersistMileageTime_POST\n\nError: " + ex.Message + "\n\n" + sb;
                        }
                        else
                        {
                            ViewBag.Message = "Function: UserController.PersistMileageTime_POST\n\nError: " + ex.Message + "\n\n" + sb + "\n\n" + ex.InnerException.Message;
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
                            ViewBag.Message = "Function: UserController.PersistMileageTime_POST\n\nError: " + ex.Message + "\n\nBaseException: " + ex.GetBaseException().Message;
                        }
                        else
                        {
                            ViewBag.Message = "Function: UserController.PersistMileageTime_POST\n\nError: " + ex.Message + "\n\nBaseException: " + ex.GetBaseException().Message + "\n\nInnerException: " + ex.InnerException.Message;
                        };
                    }
                    catch (Exception ex)
                    {
                        if (ex.InnerException == null)
                        {
                            ViewBag.Message = "Function: UserController.PersistMileageTime_POST\n\nError: " + ex.Message;
                        }
                        else
                        {
                            ViewBag.Message = "Function: UserController.PersistMileageTime_POST\n\nError: " + ex.Message + "\n\nBaseException: " + ex.InnerException.Message;
                        };
                    };
                } while (saveFailed);

                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            }
            ViewBag.SelectedActivity = model.SelectedActivity;
            ViewBag.SelectedSample = model.SelectedSample;
            model.IsValid = false;
            return View("DailyActivityLog", model);
        }

        [HttpGet]
        public ActionResult RedirectDailyActivityLog(string serviceLogID, string SelectedActivity = "None", string SelectedSample = "None")
        {
            try
            {
                TRN_SERVICE_TB log = _serviceLogRepo.GetServiceLog(Convert.ToInt32(serviceLogID));
                decimal totalActivityTime = Convert.ToDecimal(_activityRepo.GetTotalActivityTime(log.N_SERVICE_SYSID));
                decimal totalTime = Convert.ToDecimal(_activityRepo.GetTotalActivityTime(log.N_SERVICE_SYSID) + log.N_MILEAGE_TIME);
                DailyActivityLogViewModel viewModel = new DailyActivityLogViewModel()
                {
                    SelectedActivity = SelectedActivity,
                    SelectedSample = SelectedSample,
                    ServiceLogID = log.N_SERVICE_SYSID,
                    MileageTime = log.N_MILEAGE_TIME,
                    TotalActivityTime = totalActivityTime,
                    TotalTime = totalTime,
                    ServiceDate = log.DT_SERVICE.ToShortDateString(),
                    WorkerName = GetUserProperty(log.SZ_USER_NAME, "DisplayName"),
                    IsValid = true
                };
                return RedirectToAction("DailyActivityLog", viewModel);
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: UserController.RedirectDailyActivityLog_GET\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: UserController.RedirectDailyActivityLog_GET\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        [HttpGet]
        public ActionResult RenderActivityLog(string ActivityLogID)
        {
            try
            {
                TRN_SERVICE_X_ACTIVITY_TYPE_TB activityLog = new TRN_SERVICE_X_ACTIVITY_TYPE_TB();
                activityLog = _activityRepo.GetActivityByID(Convert.ToInt32(ActivityLogID));
                TRN_SERVICE_TB svcLog = new TRN_SERVICE_TB();
                svcLog = _serviceLogRepo.GetServiceLog(activityLog.N_SERVICE_SYSID);
                decimal totalActivityTime = Convert.ToDecimal(_activityRepo.GetTotalActivityTime(svcLog.N_SERVICE_SYSID));
                decimal totalTime = Convert.ToDecimal(_activityRepo.GetTotalActivityTime(svcLog.N_SERVICE_SYSID) + svcLog.N_MILEAGE_TIME);
                DailyActivityLogViewModel viewModel = new DailyActivityLogViewModel()
                {
                    SelectedActivity = activityLog.N_ACTIVITY_TYPE_SYSID.ToString(),
                    SelectedSample = "None",
                    ServiceLogID = svcLog.N_SERVICE_SYSID,
                    MileageTime = svcLog.N_MILEAGE_TIME,
                    TotalActivityTime = totalActivityTime,
                    TotalTime = totalTime,
                    ServiceDate = svcLog.DT_SERVICE.ToShortDateString(),
                    WorkerName = GetUserProperty(svcLog.SZ_USER_NAME, "DisplayName"),
                    IsValid = true
                };
                return RedirectToAction("DailyActivityLog", viewModel);
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: UserController.RenderActivityLog_GET\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: UserController.RenderActivityLog_GET\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        [HttpGet]
        public ActionResult DeleteActivityLog(string ActivityLogID, string Caller)
        {
            bool saveFailed;
            do
            {
                saveFailed = false;
                try
                {
                    TRN_SERVICE_X_ACTIVITY_TYPE_TB activityLog = new TRN_SERVICE_X_ACTIVITY_TYPE_TB();
                    activityLog = _activityRepo.GetActivityByID(Convert.ToInt32(ActivityLogID));
                    int serviceLogID = activityLog.N_SERVICE_SYSID;

                    IEnumerable<TRN_SAMPLE_TB> samples = _sampleRepo.GetSamplesByActivityID(activityLog.N_SERVICE_X_ACTIVITY_TYPE_SYSID);
                    foreach (TRN_SAMPLE_TB sample in samples)
                    {
                        _uow.Repository<TRN_SAMPLE_TB>().Delete(sample.N_SAMPLE_SYSID);
                    }

                    _uow.Repository<TRN_SERVICE_X_ACTIVITY_TYPE_TB>().Delete(Convert.ToInt32(ActivityLogID));
                    _uow.SaveChanges();

                    switch (Caller)
                    {
                        case "Daily Summary":
                            return RedirectToAction("RenderDailySummaryLog", "User", serviceLogID);
                        case "Activity":
                            return RedirectToAction("RenderServiceLog", "User", new { ServiceLogID = serviceLogID.ToString() });
                        default:
                            return RedirectToAction("RenderDailySummaryLog", "User", serviceLogID);
                    }
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
                        ViewBag.Message = "Function: UserController.DeleteActivityLog_GET\n\nError: " + ex.Message + "\n\n" + sb;
                    }
                    else
                    {
                        ViewBag.Message = "Function: UserController.DeleteActivityLog_GET\n\nError: " + ex.Message + "\n\n" + sb + "\n\n" + ex.InnerException.Message;
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
                        ViewBag.Message = "Function: UserController.DeleteActivityLog_GET\n\nError: " + ex.Message + "\n\nBaseException: " + ex.GetBaseException().Message;
                    }
                    else
                    {
                        ViewBag.Message = "Function: UserController.DeleteActivityLog_GET\n\nError: " + ex.Message + "\n\nBaseException: " + ex.GetBaseException().Message + "\n\nInnerException: " + ex.InnerException.Message;
                    };
                }
                catch (Exception ex)
                {
                    if (ex.InnerException == null)
                    {
                        ViewBag.Message = "Function: UserController.DeleteActivityLog_GET\n\nError: " + ex.Message;
                    }
                    else
                    {
                        ViewBag.Message = "Function: UserController.DeleteActivityLog_GET\n\nError: " + ex.Message + "\n\nBaseException: " + ex.InnerException.Message;
                    };
                };
            } while (saveFailed);

            Session["ErrorMessage"] = ViewBag.Message;
            return RedirectToAction("InternalServerError", "Error");
        }

        [HttpGet]
        public ActionResult GetDailyActivitiesByServiceLogID(string ServiceLogID)
        {
            try
            {
                var dailyActivities = _dailyActivityRepo.GetDailyActivityByServiceLogID(Convert.ToInt32(ServiceLogID)).Select(a => new
                {
                    DailyActivityLogID = a.N_SERVICE_X_ACTIVITY_TYPE_SYSID,
                    Address = a.SZ_ADDRESS == null ? "" : a.SZ_ADDRESS,
                    CivilDivision = _civilDivisionRepo.GetCivilDivisionNameByID(a.SZ_FIPS_CODE),
                    Activity = _activityRepo.GetActivityNameByID(a.N_ACTIVITY_TYPE_SYSID),
                    Program = _programRepo.GetProgramNameByID(a.N_PROGRAM_SYSID),
                    Subprogram = a.N_SUBPROGRAM_SYSID == null ? "" : _subprogramRepo.GetSubprogramNameByID((int)a.N_SUBPROGRAM_SYSID),
                    Time = a.N_TIME,
                    Identical = a.SZ_IDENTICAL_SERVICE_CODE == null ? "" : a.SZ_IDENTICAL_SERVICE_CODE,
                    Option = _optionRepo.GetOptionNameByID((int)a.N_OPTION_SYSID)
                }).ToList();
                return Json(dailyActivities, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: UserController.GetDailyActivitiesByServiceLogID_GET\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: UserController.GetDailyActivitiesByServiceLogID_GET\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        [HttpGet]
        public ActionResult GetSamplesByActivityID(string ActivityID)
        {
            try
            {
                var samples = _sampleRepo.GetSamplesByActivityID(Convert.ToInt32(ActivityID)).Select(s => new
                {
                    SampleID = s.N_SAMPLE_SYSID,
                    SampleType = _sampleRepo.GetSampleNameByID(s.N_SAMPLE_TYPE_SYSID),
                    Quantity = s.N_QUANTITY
                }).ToList();
                return Json(samples, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: UserController.GetSamplesByActivityID_GET\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: UserController.GetSamplesByActivityID_GET\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        #endregion

        #region - Activity -

        [HttpGet]
        public ActionResult InsertActivity(string ID)
        {
            try
            {
                return Json(new { result = "Redirect", url = Url.Action("Activity", "User"), value = ID });
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: UserController.InsertActivity_GET\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: UserController.InsertActivity_GET\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        [HttpGet]
        [OutputCache(Duration = 0)]
        public ActionResult Activity(string ID)
        {
            try
            {
                if (TempData["ActivityModel"] == null)
                {
                    TRN_SERVICE_TB model = _serviceLogRepo.GetServiceLog(Convert.ToInt32(ID));
                    ActivityViewModel viewModel = new ActivityViewModel()
                    {
                        ActivityLogID = 0,
                        Activities = (List<SelectListItem>)_activityRepo.GetActivities(),
                        CivilDivisions = (List<SelectListItem>)_civilDivisionRepo.GetCivilDivisions(),
                        MileageTime = model.N_MILEAGE_TIME,
                        Options = (List<SelectListItem>)_optionRepo.GetOptions(),
                        Programs = (List<SelectListItem>)_programRepo.GetPrograms(),
                        ServiceDate = model.DT_SERVICE.ToShortDateString(),
                        ServiceLogID = model.N_SERVICE_SYSID,
                        Subprograms = (List<SelectListItem>)_subprogramRepo.GetSubprograms(_programRepo.GetProgramID()),
                        WorkerName = GetUserProperty(model.SZ_USER_NAME, "DisplayName"),
                        IdenticalValidationMessage = "",
                        ShowIdenticalMessage = false,
                        TotalTime = Convert.ToDecimal(_activityRepo.GetTotalActivityTime(model.N_SERVICE_SYSID) + model.N_MILEAGE_TIME)
                    };
                    return View(viewModel);
                }
                else
                {
                    ActivityViewModel activityModel = (ActivityViewModel)TempData["ActivityModel"];
                    activityModel.Activities = (List<SelectListItem>)_activityRepo.GetActivities();
                    activityModel.CivilDivisions = (List<SelectListItem>)_civilDivisionRepo.GetCivilDivisions();
                    activityModel.Options = (List<SelectListItem>)_optionRepo.GetOptions();
                    activityModel.Programs = (List<SelectListItem>)_programRepo.GetPrograms();
                    activityModel.Subprograms = (List<SelectListItem>)_subprogramRepo.GetSubprograms(activityModel.SelectedProgram);
                    activityModel.IdenticalValidationMessage = "";
                    activityModel.ShowIdenticalMessage = false;
                    TempData["ActivityModel"] = null;
                    return View(activityModel);
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: UserController.Activity_GET\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: UserController.Activity_GET\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        [HttpPost]
        public ActionResult Activity(ActivityViewModel model)
        {
            bool saveFailed;
            do
            {
                saveFailed = false;
                try
                {
                    int n;
                    if (model.Identical == null) model.Identical = "0";
                    if (ModelState.IsValid && int.TryParse(model.Identical, out n) && model.ActivityTime > 0)
                    {
                        TRN_SERVICE_X_ACTIVITY_TYPE_TB activity;
                        TRN_SERVICE_X_ACTIVITY_TYPE_TB activityFound;
                        string activityLogID;
                        if (model.ActivityLogID == 0)
                        {
                            if (model.Address == null) model.Address = "";
                            if (model.Identical == null) model.Identical = "";
                            activity = new TRN_SERVICE_X_ACTIVITY_TYPE_TB()
                            {
                                B_INACTIVE = false,
                                DT_MODIFIED = DateTime.UtcNow,
                                SZ_MODIFIED_BY = _modifiedBy,
                                N_ACTIVITY_TYPE_SYSID = model.SelectedActivity,
                                N_OPTION_SYSID = model.SelectedOption,
                                N_PROGRAM_SYSID = model.SelectedProgram,
                                N_SERVICE_SYSID = model.ServiceLogID,
                                N_SUBPROGRAM_SYSID = model.SelectedSubprogram == 0 ? (int?)null : model.SelectedSubprogram,
                                N_TIME = (decimal)model.ActivityTime,
                                SZ_ADDRESS = model.Address,
                                SZ_IDENTICAL_SERVICE_CODE = model.Identical.ToString(),
                                SZ_CODE = "",
                                SZ_FIPS_CODE = model.SelectedCivilDivision == null ? "" : model.SelectedCivilDivision.ToString()
                            };
                            _uow.Repository<TRN_SERVICE_X_ACTIVITY_TYPE_TB>().Add(activity);
                            _uow.SaveChanges();
                            activityFound = _uow.Repository<TRN_SERVICE_X_ACTIVITY_TYPE_TB>()
                                .Find(u => u.B_INACTIVE == false
                                    && u.SZ_MODIFIED_BY == _modifiedBy
                                    && u.N_ACTIVITY_TYPE_SYSID == model.SelectedActivity
                                    && u.N_OPTION_SYSID == model.SelectedOption
                                    && u.N_PROGRAM_SYSID == model.SelectedProgram
                                    && u.N_SERVICE_SYSID == model.ServiceLogID
                                    && u.N_TIME == (decimal)model.ActivityTime
                                    && u.SZ_ADDRESS == model.Address
                                    && u.SZ_FIPS_CODE == (model.SelectedCivilDivision == null ? "" : model.SelectedCivilDivision.ToString()))
                                .FirstOrDefault();
                            activityLogID = activityFound.N_SERVICE_X_ACTIVITY_TYPE_SYSID.ToString();
                        }
                        else
                        {
                            activity = _uow.Repository<TRN_SERVICE_X_ACTIVITY_TYPE_TB>().GetById(model.ActivityLogID);
                            activity.DT_MODIFIED = DateTime.UtcNow;
                            activity.SZ_MODIFIED_BY = _modifiedBy;
                            activity.N_ACTIVITY_TYPE_SYSID = model.SelectedActivity;
                            activity.N_OPTION_SYSID = model.SelectedOption;
                            activity.N_PROGRAM_SYSID = model.SelectedProgram;
                            activity.N_SERVICE_SYSID = model.ServiceLogID;
                            activity.N_SUBPROGRAM_SYSID = model.SelectedSubprogram == 0 ? (int?)null : model.SelectedSubprogram;
                            activity.N_TIME = (decimal)model.ActivityTime;
                            activity.SZ_ADDRESS = model.Address;
                            activity.SZ_IDENTICAL_SERVICE_CODE = model.Identical == null ? "" : model.Identical.ToString();
                            activity.SZ_FIPS_CODE = model.SelectedCivilDivision == null ? "" : model.SelectedCivilDivision.ToString();
                            _uow.Repository<TRN_SERVICE_X_ACTIVITY_TYPE_TB>().Update(activity);
                            _uow.SaveChanges();
                            activityLogID = model.ActivityLogID.ToString();
                        }
                        return RedirectToAction("RedirectDailyActivityLog", "User", new { serviceLogID = model.ServiceLogID.ToString(), SelectedActivity = activityLogID });
                    }
                    TRN_SERVICE_TB serviceLog = _serviceLogRepo.GetServiceLog(Convert.ToInt32(model.ServiceLogID));
                    ActivityViewModel viewModel = new ActivityViewModel()
                    {
                        Activities = (List<SelectListItem>)_activityRepo.GetActivities(),
                        SelectedActivity = model.SelectedActivity,
                        CivilDivisions = (List<SelectListItem>)_civilDivisionRepo.GetCivilDivisions(),
                        SelectedCivilDivision = model.SelectedCivilDivision,
                        MileageTime = serviceLog.N_MILEAGE_TIME,
                        Options = (List<SelectListItem>)_optionRepo.GetOptions(),
                        SelectedOption = model.SelectedOption,
                        Programs = (List<SelectListItem>)_programRepo.GetPrograms(),
                        SelectedProgram = model.SelectedProgram,
                        ServiceDate = serviceLog.DT_SERVICE.ToShortDateString(),
                        ServiceLogID = model.ServiceLogID,
                        Subprograms = (List<SelectListItem>)_subprogramRepo.GetSubprograms(_programRepo.GetProgramID()),
                        SelectedSubprogram = model.SelectedSubprogram,
                        WorkerName = GetUserProperty(serviceLog.SZ_USER_NAME, "DisplayName"),
                        ShowIdenticalMessage = !int.TryParse(model.Identical, out n),
                        IdenticalValidationMessage = "Identical must be a numeric value.",
                        ShowActivityTimeMessage = model.ActivityTime <= 0,
                        ActivityTimeValidationMessage = "Activity Time must be a numeric value greater than zero."
                    };
                    return View(viewModel);
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
                        ViewBag.Message = "Function: UserController.Activity_POST\n\nError: " + ex.Message + "\n\n" + sb;
                    }
                    else
                    {
                        ViewBag.Message = "Function: UserController.Activity_POST\n\nError: " + ex.Message + "\n\n" + sb + "\n\n" + ex.InnerException.Message;
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
                        ViewBag.Message = "Function: UserController.Activity_POST\n\nError: " + ex.Message + "\n\nBaseException: " + ex.GetBaseException().Message;
                    }
                    else
                    {
                        ViewBag.Message = "Function: UserController.Activity_POST\n\nError: " + ex.Message + "\n\nBaseException: " + ex.GetBaseException().Message + "\n\nInnerException: " + ex.InnerException.Message;
                    };
                }
                catch (Exception ex)
                {
                    if (ex.InnerException == null)
                    {
                        ViewBag.Message = "Function: UserController.Activity_POST\n\nError: " + ex.Message;
                    }
                    else
                    {
                        ViewBag.Message = "Function: UserController.Activity_POST\n\nError: " + ex.Message + "\n\nBaseException: " + ex.InnerException.Message;
                    };
                };
            } while (saveFailed);

            Session["ErrorMessage"] = ViewBag.Message;
            return RedirectToAction("InternalServerError", "Error");
        }

        [HttpGet]
        public ActionResult RenderActivity(string ActivityLogID)
        {
            try
            {
                TRN_SERVICE_X_ACTIVITY_TYPE_TB activity = _activityRepo.GetActivityByID(Convert.ToInt32(ActivityLogID));
                TRN_SERVICE_TB serviceLog = _serviceLogRepo.GetServiceLog(activity.N_SERVICE_SYSID);
                ActivityViewModel model = new ActivityViewModel()
                {
                    ActivityLogID = Convert.ToInt32(ActivityLogID),
                    Activities = (List<SelectListItem>)_activityRepo.GetActivities(),
                    ActivityTime = activity.N_TIME,
                    Address = activity.SZ_ADDRESS,
                    SelectedActivity = activity.N_ACTIVITY_TYPE_SYSID,
                    CivilDivisions = (List<SelectListItem>)_civilDivisionRepo.GetCivilDivisions(),
                    SelectedCivilDivision = activity.SZ_FIPS_CODE,
                    Identical = activity.SZ_IDENTICAL_SERVICE_CODE,
                    MileageTime = serviceLog.N_MILEAGE_TIME,
                    Options = (List<SelectListItem>)_optionRepo.GetOptions(),
                    SelectedOption = Convert.ToInt32(activity.N_OPTION_SYSID),
                    Programs = (List<SelectListItem>)_programRepo.GetPrograms(),
                    SelectedProgram = activity.N_PROGRAM_SYSID,
                    ServiceDate = serviceLog.DT_SERVICE.ToShortDateString(),
                    ServiceLogID = serviceLog.N_SERVICE_SYSID,
                    Subprograms = (List<SelectListItem>)_subprogramRepo.GetSubprograms(activity.N_PROGRAM_SYSID),
                    SelectedSubprogram = Convert.ToInt32(activity.N_SUBPROGRAM_SYSID),
                    WorkerName = GetUserProperty(serviceLog.SZ_USER_NAME, "DisplayName"),
                    TotalTime = Convert.ToDecimal(_activityRepo.GetTotalActivityTime(serviceLog.N_SERVICE_SYSID) + serviceLog.N_MILEAGE_TIME)
                };
                TempData["ActivityModel"] = model;
                return RedirectToAction("Activity", new { ID = serviceLog.N_SERVICE_SYSID.ToString() });
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: UserController.RenderActivity_GET\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: UserController.RenderActivity_GET\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        public ActionResult LoadSubprograms(int programID)
        {
            try
            {
                List<SelectListItem> subprograms = new List<SelectListItem>();
                subprograms = (List<SelectListItem>)_subprogramRepo.GetSubprograms(programID);
                return Json(new SelectList(subprograms, "Value", "Text"));
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: UserController.LoadSubprograms\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: UserController.LoadSubprograms\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        #endregion

        #region - Activity Samples -

        [HttpGet]
        public ActionResult ActivitySamples(string ID, int SampleLogID = 0, int Quantity = 0, int SelectedSample = 0)
        {
            try
            {
                TRN_SERVICE_X_ACTIVITY_TYPE_TB activity = _activityRepo.GetActivityByID(Convert.ToInt32(ID));
                TRN_SERVICE_TB serviceLog = _serviceLogRepo.GetServiceLog(activity.N_SERVICE_SYSID);
                ActivitySamplesViewModel viewModel = new ActivitySamplesViewModel()
                {
                    Activity = _activityRepo.GetActivityNameByID(activity.N_ACTIVITY_TYPE_SYSID),
                    ActivityLogID = activity.N_SERVICE_X_ACTIVITY_TYPE_SYSID,
                    ActivityTime = activity.N_TIME,
                    Address = activity.SZ_ADDRESS,
                    CivilDivision = _civilDivisionRepo.GetCivilDivisionNameByID(activity.SZ_FIPS_CODE),
                    Identical = activity.SZ_IDENTICAL_SERVICE_CODE,
                    Option = _optionRepo.GetOptionNameByID(Convert.ToInt32(activity.N_OPTION_SYSID)),
                    Program = _programRepo.GetProgramNameByID(activity.N_PROGRAM_SYSID),
                    Quantity = Quantity,
                    SampleLogID = SampleLogID,
                    SampleTypes = (List<SelectListItem>)_sampleRepo.GetSamples(activity.N_SERVICE_X_ACTIVITY_TYPE_SYSID),
                    SelectedSampleType = SelectedSample,
                    Subprogram = _subprogramRepo.GetSubprogramNameByID(Convert.ToInt32(activity.N_SUBPROGRAM_SYSID)),
                    MileageTime = serviceLog.N_MILEAGE_TIME,
                    ServiceDate = serviceLog.DT_SERVICE.ToShortDateString(),
                    ServiceLogID = serviceLog.N_SERVICE_SYSID,
                    WorkerName = GetUserProperty(serviceLog.SZ_USER_NAME, "DisplayName")
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: UserController.ActivitySamples_GET\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: UserController.ActivitySamples_GET\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        [HttpPost]
        public ActionResult ActivitySamples(ActivitySamplesViewModel model)
        {
            bool saveFailed;
            do
            {
                saveFailed = false;
                try
                {
                    if (ModelState.IsValid)
                    {
                        TRN_SAMPLE_TB sample;
                        TRN_SAMPLE_TB sampleFound;
                        string sampleLogID;
                        if (model.SampleLogID == 0)
                        {
                            sample = new TRN_SAMPLE_TB()
                            {
                                B_INACTIVE = false,
                                DT_MODIFIED = DateTime.UtcNow,
                                N_QUANTITY = model.Quantity,
                                N_SAMPLE_TYPE_SYSID = model.SelectedSampleType,
                                N_SERVICE_X_ACTIVITY_TYPE_SYSID = model.ActivityLogID,
                                SZ_MODIFIED_BY = _modifiedBy
                            };
                            _uow.Repository<TRN_SAMPLE_TB>().Add(sample);
                            _uow.SaveChanges();
                            sampleFound = _uow.Repository<TRN_SAMPLE_TB>()
                                .Find(u => u.B_INACTIVE == false
                                    && u.N_QUANTITY == model.Quantity
                                    && u.N_SAMPLE_TYPE_SYSID == model.SelectedSampleType
                                    && u.N_SERVICE_X_ACTIVITY_TYPE_SYSID == model.ActivityLogID
                                    && u.SZ_MODIFIED_BY == _modifiedBy)
                                .FirstOrDefault();
                            sampleLogID = sampleFound.N_SAMPLE_TYPE_SYSID.ToString();
                        }
                        else
                        {
                            sample = _uow.Repository<TRN_SAMPLE_TB>().GetById(model.SampleLogID);
                            sample.DT_MODIFIED = DateTime.UtcNow;
                            sample.N_QUANTITY = model.Quantity;
                            sample.N_SAMPLE_TYPE_SYSID = model.SelectedSampleType;
                            sample.N_SERVICE_X_ACTIVITY_TYPE_SYSID = model.ActivityLogID;
                            sample.SZ_MODIFIED_BY = _modifiedBy;
                            _uow.Repository<TRN_SAMPLE_TB>().Update(sample);
                            _uow.SaveChanges();
                            sampleLogID = model.SampleLogID.ToString();
                        }
                        return RedirectToAction("RedirectDailyActivityLog", "User", new
                        {
                            serviceLogID = model.ServiceLogID.ToString(),
                            SelectedActivity = model.ActivityLogID.ToString(),
                            SelectedSample = sampleLogID
                        });
                    }
                    TRN_SERVICE_X_ACTIVITY_TYPE_TB activity = _activityRepo.GetActivityByID(Convert.ToInt32(model.ActivityLogID));
                    TRN_SERVICE_TB serviceLog = _serviceLogRepo.GetServiceLog(activity.N_SERVICE_SYSID);
                    ActivitySamplesViewModel viewModel = new ActivitySamplesViewModel()
                    {
                        Activity = _activityRepo.GetActivityNameByID(activity.N_ACTIVITY_TYPE_SYSID),
                        ActivityLogID = activity.N_SERVICE_X_ACTIVITY_TYPE_SYSID,
                        ActivityTime = activity.N_TIME,
                        Address = activity.SZ_ADDRESS,
                        CivilDivision = _civilDivisionRepo.GetCivilDivisionNameByID(activity.SZ_FIPS_CODE),
                        Identical = activity.SZ_IDENTICAL_SERVICE_CODE,
                        Option = _optionRepo.GetOptionNameByID(Convert.ToInt32(activity.N_OPTION_SYSID)),
                        Program = _programRepo.GetProgramNameByID(activity.N_PROGRAM_SYSID),
                        SampleTypes = (List<SelectListItem>)_sampleRepo.GetSamples(activity.N_SERVICE_X_ACTIVITY_TYPE_SYSID),
                        SelectedSampleType = model.SelectedSampleType,
                        Subprogram = _subprogramRepo.GetSubprogramNameByID(Convert.ToInt32(activity.N_SUBPROGRAM_SYSID)),
                        MileageTime = serviceLog.N_MILEAGE_TIME,
                        ServiceDate = serviceLog.DT_SERVICE.ToShortDateString(),
                        ServiceLogID = serviceLog.N_SERVICE_SYSID,
                        WorkerName = GetUserProperty(serviceLog.SZ_USER_NAME, "DisplayName")
                    };
                    return View(viewModel);
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
                        ViewBag.Message = "Function: UserController.ActivitySamples_POST\n\nError: " + ex.Message + "\n\n" + sb;
                    }
                    else
                    {
                        ViewBag.Message = "Function: UserController.ActivitySamples_POST\n\nError: " + ex.Message + "\n\n" + sb + "\n\n" + ex.InnerException.Message;
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
                        ViewBag.Message = "Function: UserController.ActivitySamples_POST\n\nError: " + ex.Message + "\n\nBaseException: " + ex.GetBaseException().Message;
                    }
                    else
                    {
                        ViewBag.Message = "Function: UserController.ActivitySamples_POST\n\nError: " + ex.Message + "\n\nBaseException: " + ex.GetBaseException().Message + "\n\nInnerException: " + ex.InnerException.Message;
                    };
                }
                catch (Exception ex)
                {
                    if (ex.InnerException == null)
                    {
                        ViewBag.Message = "Function: UserController.ActivitySamples_POST\n\nError: " + ex.Message;
                    }
                    else
                    {
                        ViewBag.Message = "Function: UserController.ActivitySamples_POST\n\nError: " + ex.Message + "\n\nBaseException: " + ex.InnerException.Message;
                    };
                };
            } while (saveFailed);

            Session["ErrorMessage"] = ViewBag.Message;
            return RedirectToAction("InternalServerError", "Error");
        }

        [HttpGet]
        public ActionResult RenderSampleLog(string SampleLogID)
        {
            try
            {
                TRN_SAMPLE_TB sample = _sampleRepo.GetSampleBySampleID(Convert.ToInt32(SampleLogID));
                return RedirectToAction("ActivitySamples", new { ID = sample.N_SERVICE_X_ACTIVITY_TYPE_SYSID.ToString(), SampleLogID = SampleLogID, Quantity = sample.N_QUANTITY, SelectedSample = sample.N_SAMPLE_TYPE_SYSID });
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: UserController.RenderSampleLog_GET\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: UserController.RenderSampleLog_GET\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        [HttpGet]
        public ActionResult DeleteSampleLog(string SampleLogID, string Caller)
        {
            bool saveFailed;
            do
            {
                saveFailed = false;
                try
                {
                    TRN_SAMPLE_TB sample = _sampleRepo.GetSampleBySampleID(Convert.ToInt32(SampleLogID));
                    TRN_SERVICE_X_ACTIVITY_TYPE_TB activityLog = _activityRepo.GetActivityByID(sample.N_SERVICE_X_ACTIVITY_TYPE_SYSID);
                    int serviceLogID = activityLog.N_SERVICE_SYSID;
                    _uow.Repository<TRN_SAMPLE_TB>().Delete(Convert.ToInt32(SampleLogID));
                    _uow.SaveChanges();
                    switch (Caller)
                    {
                        case "Daily Summary":
                            return RedirectToAction("RenderDailySummaryLog", "User", serviceLogID);
                        case "Activity":
                            return RedirectToAction("RenderServiceLog", "User", new { ServiceLogID = serviceLogID.ToString(), SelectedActivity = activityLog.N_SERVICE_X_ACTIVITY_TYPE_SYSID.ToString() });
                        default:
                            return RedirectToAction("RenderDailySummaryLog", "User", serviceLogID);
                    }
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
                        ViewBag.Message = "Function: UserController.DeleteSampleLog_GET\n\nError: " + ex.Message + "\n\n" + sb;
                    }
                    else
                    {
                        ViewBag.Message = "Function: UserController.DeleteSampleLog_GET\n\nError: " + ex.Message + "\n\n" + sb + "\n\n" + ex.InnerException.Message;
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
                        ViewBag.Message = "Function: UserController.DeleteSampleLog_GET\n\nError: " + ex.Message + "\n\nBaseException: " + ex.GetBaseException().Message;
                    }
                    else
                    {
                        ViewBag.Message = "Function: UserController.DeleteSampleLog_GET\n\nError: " + ex.Message + "\n\nBaseException: " + ex.GetBaseException().Message + "\n\nInnerException: " + ex.InnerException.Message;
                    };
                }
                catch (Exception ex)
                {
                    if (ex.InnerException == null)
                    {
                        ViewBag.Message = "Function: UserController.DeleteSampleLog_GET\n\nError: " + ex.Message;
                    }
                    else
                    {
                        ViewBag.Message = "Function: UserController.DeleteSampleLog_GET\n\nError: " + ex.Message + "\n\nBaseException: " + ex.InnerException.Message;
                    };
                };
            } while (saveFailed);

            Session["ErrorMessage"] = ViewBag.Message;
            return RedirectToAction("InternalServerError", "Error");
        }

        #endregion

        #region - Daily Summary Log -

        [HttpGet]
        [OutputCache(Duration = 0)]
        public ActionResult DailySummaryLog()
        {
            try
            {
                List<SelectListItem> workerList = null;
                List<User> userList = GetUsersByDepartment("{Department:'" + ConfigurationManager.AppSettings["Application.Department"] +
                    "',Description:'" + ConfigurationManager.AppSettings["Application.Description"] + "'}");
                if (User.HasAction("AdministerAll"))
                {
                    workerList = (List<SelectListItem>)userList
                            .Select(u => new SelectListItem
                            {
                                Value = u.PrincipalName.ToString(),
                                Text = (u.DisplayName.ToString() + "  /  " + GetUserProperty(u.PrincipalName, "SAMAccountName"))
                            })
                            .OrderBy(u => u.Text)
                            .ToList();
                }
                else
                {
                    workerList = (List<SelectListItem>)userList
                            .Where(u => u.PrincipalName == User.Identity.Name)
                            .Select(u => new SelectListItem
                            {
                                Value = u.PrincipalName.ToString(),
                                Text = (u.DisplayName.ToString() + "  /  " + GetUserProperty(u.PrincipalName, "SAMAccountName"))
                            })
                            .OrderBy(u => u.Text)
                            .ToList();
                }

                // workaround for testing - add current user to dropdown
                //if (!workerList.Select(u => u.Text).Contains(SessionHelper.DisplayName + " / " + GetUserProperty(SessionHelper.UserName, "SAMAccountName")))
                //{
                //    var item = new SelectListItem { Value = SessionHelper.UserName, Text = (SessionHelper.DisplayName + " / " + GetUserProperty(SessionHelper.UserName, "SAMAccountName")) };
                //    workerList.Add(item);
                //    workerList = workerList.OrderBy(u => u.Value).ToList();
                //}

                DailySummaryLogViewModel viewModel = new DailySummaryLogViewModel()
                {
                    Workers = workerList
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: UserController.DailySummaryLog_GET\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: UserController.DailySummaryLog_GET\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        [HttpGet]
        public ActionResult RenderDailySummaryLog()
        {
            try
            {
                return RedirectToAction("DailySummaryLog");
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: UserController.RenderDailySummaryLog_GET\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: UserController.RenderDailySummaryLog_GET\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        [HttpGet]
        public ActionResult GetServiceLogsDateRangeByWorkerID(string WorkerPrincipalName, string StartDate, string EndDate)
        {
            try
            {
                DateTime startDate = DateTime.Parse(StartDate);
                DateTime endDate = DateTime.Parse(EndDate);
                if (startDate > endDate)
                {
                    startDate = DateTime.Parse(EndDate);
                    endDate = DateTime.Parse(StartDate);
                }
                var serviceLogs = _serviceLogRepo.GetServiceLogsDateRangeByWorkerID(startDate, endDate, WorkerPrincipalName).Select(a => new
                {
                    ServiceLogID = a.N_SERVICE_SYSID,
                    WorkerName = GetUserProperty(WorkerPrincipalName, "DisplayName"),
                    ServiceDate = a.DT_SERVICE.ToShortDateString(),
                    MileageTime = a.N_MILEAGE_TIME == null ? 0 : a.N_MILEAGE_TIME,
                    a.DT_SERVICE.Month,
                    a.DT_SERVICE.Day,
                    a.DT_SERVICE.Year
                }).ToList().OrderByDescending(u => u.Year).ThenByDescending(u => u.Month).ThenByDescending(u => u.Day);
                return Json(serviceLogs, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: UserController.GetServiceLogsDateRangeByWorkerID_GET\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: UserController.GetServiceLogsDateRangeByWorkerID_GET\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        [HttpGet]
        public ActionResult GetDailyActivityLogsByServiceLogID(string ServiceLogID)
        {
            try
            {
                var activityLogs = _dailyActivityRepo.GetDailyActivityByServiceLogID(Convert.ToInt32(ServiceLogID)).Select(a => new
                {
                    DailyActivityLogID = a.N_SERVICE_X_ACTIVITY_TYPE_SYSID,
                    Address = a.SZ_ADDRESS == null ? "" : a.SZ_ADDRESS,
                    CivilDivision = _civilDivisionRepo.GetCivilDivisionNameByID(a.SZ_FIPS_CODE),
                    Activity = _activityRepo.GetActivityNameByID(a.N_ACTIVITY_TYPE_SYSID),
                    Program = _programRepo.GetProgramNameByID(a.N_PROGRAM_SYSID),
                    Subprogram = _subprogramRepo.GetSubprogramNameByID(Convert.ToInt32(a.N_SUBPROGRAM_SYSID)),
                    Time = a.N_TIME,
                    Option = _optionRepo.GetOptionNameByID(Convert.ToInt32(a.N_OPTION_SYSID)),
                    Identical = a.SZ_IDENTICAL_SERVICE_CODE
                }).ToList();
                return Json(activityLogs, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: UserController.GetDailyActivityLogsByServiceLogID_GET\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: UserController.GetDailyActivityLogsByServiceLogID_GET\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        [HttpGet]
        public ActionResult GetSampleLogsByActivityLogID(string ActivityID)
        {
            try
            {
                var sampleLogs = _sampleRepo.GetSamplesByActivityID(Convert.ToInt32(ActivityID)).Select(a => new
                {
                    SampleLogID = a.N_SAMPLE_SYSID,
                    SampleName = _sampleRepo.GetSampleNameByID(Convert.ToInt32(a.N_SAMPLE_TYPE_SYSID)),
                    SampleQuantity = a.N_QUANTITY
                }).ToList();
                return Json(sampleLogs, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ViewBag.Message = "Function: UserController.GetSampleLogsByActivityLogID_GET\n\nError: " + ex.Message;
                }
                else
                {
                    ViewBag.Message = "Function: UserController.GetSampleLogsByActivityLogID_GET\n\nError: " + (ex.Message + "\n\nInnerException: " + ex.InnerException.Message);
                };
                Session["ErrorMessage"] = ViewBag.Message;
                return RedirectToAction("InternalServerError", "Error");
            };
        }

        #endregion

        #region - Private Helper Functions -

        /*
        private void SetupAuthorization()
        {
            Uri authorizationServerUri = new Uri(ConfigurationManager.AppSettings["OAuth.MacombCounty.Paths.Remote.AuthorizationServerBaseAddress"]);
            AuthorizationServerDescription authorizationServer = new AuthorizationServerDescription()
            {
                AuthorizationEndpoint = new Uri(authorizationServerUri, ConfigurationManager.AppSettings["OAuth.MacombCounty.Paths.Relative.Authorize"]),
                TokenEndpoint = new Uri(authorizationServerUri, ConfigurationManager.AppSettings["OAuth.MacombCounty.Paths.Relative.Token"])
            };
            _webServerClient = new WebServerClient(authorizationServer, ConfigurationManager.AppSettings["OAuth.MacombCounty.ConsumerKey"], ConfigurationManager.AppSettings["OAuth.MacombCounty.ConsumerSecret"]);

            _client = new HttpClient(_webServerClient.CreateAuthorizingHandler(SessionHelper.AccessToken));
            _client.DefaultRequestHeaders.Add("Origin", Request.Url.GetLeftPart(UriPartial.Authority) + "/");
        }
        */

        private void SetUserRole()
        {
            /*
            bool isAdmin = false;
            SetupAuthorization();
            List<string> roleList = GetUserRoleList(SessionHelper.UserName);
            if (roleList.Count != 0)
            {
                if (roleList.Count == 1)
                {
                    SessionHelper.UserRole = roleList[0].ToString();
                }
                else
                {
                    foreach (string role in roleList)
                    {
                        if (role == "Administrator") isAdmin = true;
                    }
                    if (isAdmin)
                    {
                        SessionHelper.UserRole = "System Administrator";
                    }
                    else
                    {
                        SessionHelper.UserRole = "Member";
                    }
                }
            }
            */
            if (SessionHelper.AccessToken == null)
                JwtHelper.SetSessionFromTokenCookie(Request, "MC-SSO-AT");
            var role = JwtHelper.GetTokenValueByKey(SessionHelper.AccessToken, "ApplicationRole.Global");
            if (string.IsNullOrEmpty(role))
                role = JwtHelper.GetTokenValueByKey(SessionHelper.AccessToken, "ApplicationRole.EH.TimeTrack");
            if (role == "Administrator")
                role = "System Administrator";
            if (role != "System Administrator")
                role = "Member";
            SessionHelper.UserRole = role;
        }

        private List<User> GetUsersByDepartment(string FilteredProperties)
        {
            /*
            SetupAuthorization();
            string response = _client.GetStringAsync(new Uri(_resourceServerUri, "/api/User/" + SessionHelper.UserName
                + "/ListByFilteredProperties?properties=" + FilteredProperties + "")).Result;

            List<User> userList = JsonConvert.DeserializeObject<List<User>>(response.Replace("\\\"", "'").Replace("\"", ""));
            */
            var db = new ApplicationServicesCoreEntities();
            var applicationDepartment = ConfigurationManager.AppSettings["Application.Department"];
            var applicationDescription = ConfigurationManager.AppSettings["Application.Description"].Replace("*", "%");
            var users = db.Database.SqlQuery(typeof(User), "SELECT p.sz_personnel_code AS CommonName, d.sz_name AS Department, p.sz_description AS Description, c.sz_denormalized AS DisplayName, '' AS Domain, '' AS Email, c.sz_first_name AS FirstName, '' AS LastLogon, c.sz_last_name AS LastName, '' AS Manager, '' AS ManagerList, '' AS OrganizationUnitList, '' AS Phone, p.sz_principal_name AS PrincipalName, p.sz_title AS Title, '' AS UserId, p.sz_account_name AS UserName FROM [Department] d INNER JOIN [Personnel] p ON d.sz_department_id = p.sz_department_id AND p.sz_description LIKE '" + applicationDescription + "' AND p.b_inactive = 0 INNER JOIN [Contact] c ON p.sz_contact_id = c.sz_contact_id AND c.b_inactive = 0 WHERE d.b_inactive = 0 AND d.sz_name = '" + applicationDepartment + "'");
            var userList = new List<User>();
            foreach (var user in users)
            {
                userList.Add((User)user);
            }

            return userList;
        }

        private string GetUserProperty(string UserName, string PropertyName)
        {
            /*
            SetupAuthorization();
            string response = _client.GetStringAsync(new Uri(_resourceServerUri, "/api/User/" + UserName + "/Property?propertyName=" 
                + PropertyName + "")).Result;
            return response.Replace("\"", "");
            */

            var property = string.Empty;
            var db = new ApplicationServicesCoreEntities();
            switch (PropertyName)
            {
                case "DisplayName":
                    property = db.Database.SqlQuery<string>("SELECT c.sz_denormalized FROM [Personnel] p INNER JOIN [Contact] c ON p.sz_contact_id = c.sz_contact_id AND c.b_inactive = 0 WHERE p.b_inactive = 0 AND p.sz_principal_name = '" + UserName + "'").FirstOrDefault();
                    break;
                case "SAMAccountName":
                    property = db.Database.SqlQuery<string>("SELECT p.sz_account_name FROM [Personnel] p INNER JOIN [Contact] c ON p.sz_contact_id = c.sz_contact_id AND c.b_inactive = 0 WHERE p.b_inactive = 0 AND p.sz_principal_name = '" + UserName + "'").FirstOrDefault();
                    break;
            }

            return property;
        }

        /*
        private List<string> GetUserRoleList(string UserName, string ApplicationName = "EH.TimeTrack")
        {
            SetupAuthorization();
            string response = _client.GetStringAsync(new Uri(_resourceServerUri, "/api/User/" + UserName + 
                "/RoleList?&application=" + ApplicationName + "")).Result;
            List<string> roleList = JsonConvert.DeserializeObject<List<string>>(response.Replace("\\\"", "'").Replace("\"", ""));

            return roleList;
        }
        */

        #endregion
    }
}