using System;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using BookMark.RestApi.Models;
using BookMark.Client.Models;
using BookMark.Client.Utils;

namespace BookMark.Client.Controllers {
	public class EventController : Controller {
		private readonly HttpService _service;
		public EventController(HttpService service) {
			_service = service;
		}
		private async Task<Organization> GetCurrentOrg() {
			string org_id = HttpContext.Session.GetString("OrgID");
			if (org_id == null || org_id.Length == 0) {
				return null;
			}
			long ID = 0;
			if (!long.TryParse(org_id, out ID)) {
				return null;
			}
			HttpResponseMessage response = await _service.client.GetAsync($"/api/org/{ID}");
			if (!response.IsSuccessStatusCode) {
				return null;
			}
			return await response.Content.ReadAsAsync<Organization>();
		}
    private async Task<User> GetCurrentUser() {
			string acct_id = HttpContext.Session.GetString("AcctID");
			if (acct_id == null || acct_id.Length == 0) {
				return null;
			}
			long ID = 0;
			if (!long.TryParse(acct_id, out ID)) {
				return null;
			}
			HttpResponseMessage response = await _service.client.GetAsync($"/api/user/{ID}");
			if (!response.IsSuccessStatusCode) {
				return null;
			}
			return await response.Content.ReadAsAsync<User>();
		}

		private async Task<List<Event>> GetAllEvents() {
			HttpResponseMessage response = await _service.client.GetAsync($"/api/event");
			if (!response.IsSuccessStatusCode) {
				return null;
			}
			return await response.Content.ReadAsAsync<List<Event>>();
		}
		private async Task<List<UserEvent>> GetUserEvents() {
			HttpResponseMessage response = await _service.client.GetAsync("/api/user/events");
			if (!response.IsSuccessStatusCode) {
				return null;
			}
			return await response.Content.ReadAsAsync<List<UserEvent>>();
		}
		/*
		private async Task<bool> CreateNewUserEvent(User user, Event ev)
		{
			HttpContent contentuser = new StringContent(
				JsonConvert.SerializeObject(user),
				Encoding.UTF8,
				"application/json"
			);
			HttpContent contentevent = new StringContent(
				JsonConvert.SerializeObject(ev),
				Encoding.UTF8,
				"application/json"
			);
			HttpResponseMessage responseuser = await _service.client.PutAsync("/api/user", contentuser);	
			if (!responseuser.IsSuccessStatusCode) {
				return false;
			}
			HttpResponseMessage responseevent = await _service.client.PutAsync("/api/event", contentevent);	
			if (!responseevent.IsSuccessStatusCode) {
				return false;
			}
			return true;
		}
		*/

		private async Task<bool> CreateNewUserEvent(long userid, long evid) {
			UserEvent ue = new UserEvent() {
				UserID = userid,
				EventID = evid
			};

			string serial = JsonConvert.SerializeObject(ue);
			System.Console.WriteLine(serial);
			HttpContent content = new StringContent(
				serial,
				Encoding.UTF8,
				"application/json"
			);
			HttpResponseMessage response = await _service.client.PostAsync("/api/event/userevent", content);
			//TODO: ?

			if (!response.IsSuccessStatusCode) {
				return false;
			}
			return true;
		}


    
		private List<Event> GetEventsNotJoined() 
		{
			Task<User> user_task = GetCurrentUser();
			user_task.Wait();
			User user = user_task.Result;

			Task<List<Event>> event_task = GetAllEvents();
			event_task.Wait();
			List<Event> event_list = event_task.Result;

			Task<List<UserEvent>> user_event_task = GetUserEvents();
			user_event_task.Wait();
			List<UserEvent> user_event_list = user_event_task.Result;

			if (event_list == null) {
				return new List<Event>();
			}
			return event_list;

			// if (event_list != null && user_event_list != null) {
			// 	if (event_list.Count != 0) {
			// 		foreach (Event e in event_list) {
			// 			foreach (UserEvent ue in user_event_list) {
			// 				if (ue.UserID == user.UserID && ue.EventID == e.EventID) {
			// 					event_list.Remove(e);
			// 				}
			// 			}
			// 		}
			// 		return event_list;
			// 	}
			// 	return new List<Event>();
			// }
			// return new List<Event>();

			// if (eventlist != null)
			// {
			// 	if (eventlist.Count > 0)
			// 	{
			// 		List<UserEvent> uelist = 

			// 		foreach (Event e in eventlist)
			// 		{
			// 			if (e.UserEvents != null) {
			// 				foreach (UserEvent ue in e.UserEvents) {
			// 					if(user.UserID == ue.UserID)
			// 					{
			// 						eventlist.Remove(e);
			// 					}
			// 				}
			// 			}			
			// 		}
			// 		return eventlist;
			// 	}
			// 	else return new List<Event>();
			// }
			// return new List<Event>();
		}
		// TODO: TEST: Shows the events that user can rsvp
    [HttpGet]
		public IActionResult Index() 
		{
      Task<User> user_task = GetCurrentUser();
			user_task.Wait();
			User user = user_task.Result;

      // Get all Events that user is not rsvp to
      if (user != null) 
			{
        return View(CreateEVMList(GetEventsNotJoined()));
			}
			return Redirect("/home/index");
		}

		// TODO: TEST
    [HttpGet, ActionName("RSVP")]
		public IActionResult RSVP(long id) 
		{
      Task<User> user_task = GetCurrentUser();
			user_task.Wait();
			User user = user_task.Result;
      // Get event details
      if (user != null) 
			{
        Task<Event> event_task = GetEvent(id);
				event_task.Wait();
				Event ev = event_task.Result;

				return View(new EventViewModel(ev));
			}
			return Redirect("/home/index");
		}
		
		// Confirmation page after user click RSVP
		[HttpGet, ActionName("RSVPConfirmation")]
		public IActionResult RSVPConfirmation(long id) 
		{
      Task<User> user_task = GetCurrentUser();
			user_task.Wait();
			User user = user_task.Result;
			
      if (user != null) 
			{
      // TODO: post to userevent


				Task<Event> event_task = GetEvent(id);
				event_task.Wait();
				Event ev = event_task.Result;
        /*
				if (user.UserEvents == null)
				{
					user.UserEvents = new List<UserEvent>();
				}
				if (ev.UserEvents == null)
				{
					ev.UserEvents = new List<UserEvent>();
				}
				user.UserEvents.Add(new UserEvent(){
					UserID = user.UserID,
					EventID = ev.EventID
				});
				ev.UserEvents.Add(new UserEvent(){
					UserID = user.UserID,
					EventID = ev.EventID
				});
				Task<bool> a = CreateNewUserEvent(user, ev);
				*/
				Task<bool> post = CreateNewUserEvent(user.UserID, ev.EventID);
				post.Wait();
				// TODO: Test
				if (post.Result)
				{
					return View(new EventViewModel(ev));
				}
				else 
				{
					return View("RSVP", new EventViewModel(ev));
				}
			}
			return Redirect("/home/index");
		}
		
		// TODO: TEST
		// go back to either event/userevents or event/orgevents from details
		[HttpGet, ActionName("Back")]
		public IActionResult Back() 
		{
      Task<User> user_task = GetCurrentUser();
			user_task.Wait();
			User user = user_task.Result;
			Task<Organization> org_task = GetCurrentOrg();
			org_task.Wait();
			Organization org = org_task.Result;

      // Get all Events for Current User
      if (user != null) 
			{
        // update if we update login to user email
				Task<List<Event>> ev_task = FindUserEvents(user.Name);
			  ev_task.Wait();
			  List<Event> events = ev_task.Result;	

        return View("UserEvents", CreateEVMList(events));
			}
			else if (org != null) 
			{
				Task<List<Event>> ev_task = SearchEventByEmail(org.Email);
				ev_task.Wait();
        List<Event> events = ev_task.Result;	

				return View("OrgEvents", CreateEVMList(events));
			}
			return Redirect("/home/index");
		}


		[HttpGet]
		public IActionResult UserEvents() 
		{
      Task<User> user_task = GetCurrentUser();
			user_task.Wait();
			User user = user_task.Result;
      // Get all Events for Current User
      if (user != null) 
			{
        // update if we update login to user email
				Task<List<Event>> ev_task = FindUserEvents(user.Name);
			  ev_task.Wait();
			  List<Event> events = ev_task.Result;	

        return View(CreateEVMList(events));
			}
			return Redirect("/home/index");
		}

		[HttpGet]
		public IActionResult OrgEvents() 
		{
			Task<Organization> org_task = GetCurrentOrg();
			org_task.Wait();
			Organization org = org_task.Result;

      // Get all Events for Current Org
			if (org != null) 
			{
				Task<List<Event>> ev_task = SearchEventByEmail(org.Email);
				ev_task.Wait();
        List<Event> events = ev_task.Result;	

				return View(CreateEVMList(events));
			}
			return Redirect("/home/index");
		}
		
		

		public List<EventViewModel> CreateEVMList(List<Event> events)
		{
			List<EventViewModel> eventsview = new List<EventViewModel>();
			if(events != null && events.Count > 0)
			{
				foreach (var e in events)
				{
					eventsview.Add(new EventViewModel(e));
				}
			}
			return eventsview;
		}

		// (you need context to access session stuff)
		// (you need client to make HTTP requests)
    	// Get events for a user
		private async Task<List<Event>> FindUserEvents(string name) {
			HttpResponseMessage response = await _service.client.GetAsync($"/api/event/user/{name}");
			if (!response.IsSuccessStatusCode) {
				return null;
			}
			return await response.Content.ReadAsAsync<List<Event>>();
		}
    	// Get events for a organization
		private async Task<List<Event>> SearchEventByEmail(string email) {
			HttpResponseMessage response = await _service.client.GetAsync($"/api/event/org/{email}");
			if (!response.IsSuccessStatusCode) {
				return null;
			}
			return await response.Content.ReadAsAsync<List<Event>>();
		}
		private async Task<List<Event>> SearchEventByName(string name) {
			HttpResponseMessage response = await _service.client.GetAsync($"/api/event/search/{name}");
			if (!response.IsSuccessStatusCode) {
				return null;
			}
			return await response.Content.ReadAsAsync<List<Event>>();
		}
    [HttpGet]
		public IActionResult Create() {
			return View(new EventViewModel());
		}

		[HttpPost]
		public IActionResult Create(EventViewModel model) {
			if (!ModelState.IsValid) {
				return View(model);
			}
			Task<long> task_long = CreateEvent(model.Name, model.Location, model.Info, model.DateTime);
			task_long.Wait();
			long result = task_long.Result;
			if (result != 0) {
				// TODO: TEST
				Task<Event> event_task = GetEvent(result);
				event_task.Wait();
				Event ev = event_task.Result;
				return View("Confirmation", new EventViewModel(ev));
			}

			// return Redirect("/organization/index");
			return View(model);
		}
		private async Task<long> CreateEvent(string name, string location, string info, DateTime datetime) {
			Task<Organization> task_org = GetCurrentOrg();
			task_org.Wait();
			Organization org = task_org.Result;
			if (org == null) {
				return 0;
			}
			Event ev = new Event() {
				Name = name,	
				Location = location,
				DateTime = datetime,
				Info = info,
				// Organization = org,

				// FIXME:
				OrganizationID = org.OrganizationID
			};
			long EventID = ev.EventID;
			string serial = JsonConvert.SerializeObject(ev);
			System.Console.WriteLine(serial);
			HttpContent content = new StringContent(
				serial,
				Encoding.UTF8,
				"application/json"
			);
			HttpResponseMessage response = await _service.client.PostAsync("/api/event", content);
			//TODO: ?

			if (!response.IsSuccessStatusCode) {
				return 0;
			}
			return EventID;
		}
		// TODO: TEST
		[HttpGet, ActionName("Detail")]
		public IActionResult Detail(long id) 
		{
      Task<Event> event_task = GetEvent(id);
			event_task.Wait();
			Event ev = event_task.Result;

			return View(new EventViewModel(ev));
		}
		// FIXME: PASS ID AS LONG?
		private async Task<Event> GetEvent(long idlong) 
		{
			string id = idlong.ToString();
			HttpResponseMessage response = await _service.client.GetAsync($"/api/event/{id}");
			if (!response.IsSuccessStatusCode) {
				return null;
			}
			return await response.Content.ReadAsAsync<Event>();
		}

	}
}