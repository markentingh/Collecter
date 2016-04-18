using System;
using System.Collections.Generic;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Features;

namespace Collector
{
    public class Core
    {
        public string Version = "0.0.1";

        public Server Server;
        public Pipeline.App App;
        public Pipeline.WebService WebService;
        public Utility.Util Util;
        public Sql Sql;
        public User User;
        public Page Page;
        public HttpContext Context;
        public HttpRequest Request;
        public HttpResponse Response;
        public ISession Session;

        public bool isFirstLoad = false;
        public bool isLocal = false;
        public bool isWebService = false;
        public bool useViewState = true;
        public string ViewStateId = "";
        private int viewStateTimeout = 10; //minutes

        public Core(Server server, HttpContext context, string viewstate = "", string type = "", string page = "")
        {
            Server = server;
            Context = context;
            Request = context.Request;
            Response = context.Response;
            Session = context.Session;
            Sql = new Sql(this);
            Util = new Utility.Util(this);
            User = new User();

            //load viewstate
            if (useViewState == true)
            {
                ViewStateId = viewstate;
                if(ViewStateId == "") { ViewStateId = Util.Str.CreateID(); }
                if (Session.Get("viewstate-" + ViewStateId) != null)
                {
                    ViewState vs = new ViewState();
                    vs = (ViewState)Util.Serializer.ReadObject(Util.Str.GetString(Session.Get("viewstate-" + ViewStateId)), vs.GetType());
                    Page = vs.Page;

                }
            }
            if(Page == null)
            {
                //load page class
                if(page != "")
                {
                    string className = "Collector.Pages." + page;
                    Type classType = Type.GetType(className);
                    Page = (Page)Activator.CreateInstance(classType, new object[] {});
                }
                else
                {
                    Page = new Page();
                }
            }

            if (Session.Get("user") != null)
            {
                User = (User)Util.Serializer.ReadObject(Util.Str.GetString(Session.Get("user")), User.GetType());
            }

            //load references to Core S
            Sql.Load();
            Page.Load(this);
            User.Load(this);

            if (Session.Get("user") == null) {
                //initialize session (to prevent bug where session tries to initialize 
                //within S.Unload() after the response is sent to the client)
                Session.Set("user", Util.Serializer.WriteObject(User));
            }

            //detect request type & host type
            if (type == "service") { isWebService = true; }
            isLocal = Server.isLocal;
        }

        public void Unload()
        {
            Session.Set("user", Util.Serializer.WriteObject(User));
            SaveViewState();
            Sql.Close();
        }

        public void SaveViewState()
        {
            if(useViewState == false) { return; }
            //save viewstate data into session, then add/update viewstate details in viewstates list
            ViewState vs = new ViewState();
            vs.Load(this);
            Session.Set("viewstate-" + ViewStateId, Util.Serializer.WriteObject(vs));

            //get list of viewstates to update details
            structViewStateInfo vsd = new structViewStateInfo();
            ViewStates vss = new ViewStates();
            bool isfound = false;

            vsd.dateCreated = DateTime.Now;
            vsd.dateModified = DateTime.Now;
            vsd.id = ViewStateId;

            if (Util.IsEmpty(Session.Get("viewstates")) == false)
            {
                vss = (ViewStates)Util.Serializer.ReadObject(Util.Str.GetString(Session.Get("viewstates")), vss.GetType());
                if (vss.Views.Count >= 0)
                {
                    List<int> removes = new List<int>();
                    for (int x = 0; x <= vss.Views.Count - 1; x++)
                    {
                        if ((vss.Views[x].id == ViewStateId))
                        {
                            //update current vewstate modified date
                            vsd = vss.Views[x]; 
                            vsd.dateModified = DateTime.Now;
                            vss.Views[x] = vsd;
                            isfound = true;
                        }
                        else
                        {
                            //clean up expired viewstates
                            TimeSpan ts = DateTime.Now - vss.Views[x].dateModified;
                            if (ts.Minutes > viewStateTimeout)
                            {
                                removes.Add(x);
                                Session.Remove("viewstate-" + vss.Views[x].id);
                            }
                        }
                    }

                    if (removes.Count > 0)
                    {
                        //remove expired viewstates from list
                        int offset = 0;
                        foreach (int x in removes)
                        {
                            vss.Views.Remove(vss.Views[x - offset]);
                            offset += 1;
                        }
                    }
                }
            }

            if (isfound == false)
            {
                vss.Views.Add(vsd);
            }
            Session.Set("viewstates", Util.Serializer.WriteObject(vss));
        }

        public bool isSessionLost()
        {
            //if(Page.websiteTitle == "") {
            //    return true;
            //}
            return false;
        }
    }
}
