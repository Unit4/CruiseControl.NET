using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ThoughtWorks.CruiseControl.CCTrayLib.Configuration;
using ThoughtWorks.CruiseControl.CCTrayLib.Monitoring;

namespace ThoughtWorks.CruiseControl.CCTrayLib.Presentation
{
    public class ProjectStatusListViewItemAdaptor
    {
        private readonly IDetailStringProvider detailStringProvider;
        private readonly ListViewItem item = new ListViewItem();
        private readonly ListViewItem.ListViewSubItem activity;
        private readonly ListViewItem.ListViewSubItem detail;
        private readonly ListViewItem.ListViewSubItem lastBuildLabel;
        private readonly ListViewItem.ListViewSubItem lastBuildTime;
        private readonly ListViewItem.ListViewSubItem projectStatus;
        private readonly ListViewItem.ListViewSubItem serverName;
        private readonly ListViewItem.ListViewSubItem category;
        private readonly ListViewItem.ListViewSubItem qName;
        private readonly ListViewItem.ListViewSubItem qPriority;


        private readonly ICCTrayMultiConfiguration config = null;
        private readonly ListView listView = null;
        private readonly IList<ProjectState> hideStates = null;
        private readonly Dictionary<string, bool> showWithFlags = null;

        private bool removed = false;
        

        public ProjectStatusListViewItemAdaptor(IDetailStringProvider detailStringProvider, ICCTrayMultiConfiguration config, ListView listView, IList<ProjectState> hideStates, Dictionary<string, bool> showWithFlags)
            : this(detailStringProvider)
        {
            this.config = config;
            this.listView = listView;
            this.hideStates = hideStates;
            this.showWithFlags = showWithFlags;
        }

        public ProjectStatusListViewItemAdaptor(IDetailStringProvider detailStringProvider)
        {
            this.detailStringProvider = detailStringProvider;
            serverName = new ListViewItem.ListViewSubItem();
            item.SubItems.Add(serverName);
            category = new ListViewItem.ListViewSubItem(item, string.Empty);
            item.SubItems.Add(category);
            activity = new ListViewItem.ListViewSubItem(item, string.Empty);
            item.SubItems.Add(activity);
            detail = new ListViewItem.ListViewSubItem(item, string.Empty);
            item.SubItems.Add(detail);
            lastBuildLabel = new ListViewItem.ListViewSubItem(item, string.Empty);
            item.SubItems.Add(lastBuildLabel);
            lastBuildTime = new ListViewItem.ListViewSubItem(item, string.Empty);
            item.SubItems.Add(lastBuildTime);
            projectStatus = new ListViewItem.ListViewSubItem(item, string.Empty);
            item.SubItems.Add(projectStatus);
            qName = new ListViewItem.ListViewSubItem(item, string.Empty);
            item.SubItems.Add(qName);
            qPriority = new ListViewItem.ListViewSubItem(item, string.Empty);
            item.SubItems.Add(qPriority);

        }

        public ListViewItem Create(IProjectMonitor projectMonitor)
        {
            projectMonitor.Polled += new MonitorPolledEventHandler(Monitor_Polled);

            item.Text = projectMonitor.Detail.ProjectName;

            DisplayProjectStateInListViewItem(projectMonitor);

            return item;
        }

        private void Monitor_Polled(object sauce, MonitorPolledEventArgs args)
        {
            bool showDisconnected;
            this.showWithFlags.TryGetValue("disconnected", out showDisconnected);
            if (hideStates.Contains(args.ProjectMonitor.ProjectState) || !showDisconnected && !args.ProjectMonitor.IsConnected)
            {
                this.removed = true;
                item.Remove();
            }
            else if (removed) {
                removed = false;
                listView.Items.Add(item);
            }
            DisplayProjectStateInListViewItem(args.ProjectMonitor);
        }

        private void DisplayProjectStateInListViewItem(IProjectMonitor monitor)
        {
            item.ImageIndex = monitor.ProjectState.ImageIndex;

            if (monitor.Detail.IsConnected)
            {
                if (config != null)
                {
                    foreach (CCTrayProject project in config.Projects)
                    {
                        if ((project.ProjectName == monitor.Detail.ProjectName)
                            // add by rei , fixes issue with displaying wrong server in cctray, while multiple configured server 
                            // having project with the same project name 
                            && (project.BuildServer.DisplayName == monitor.Detail.Configuration.BuildServer.DisplayName))
                        {
                            serverName.Text = project.BuildServer.DisplayName;
                        }
                    }
                }
                else
                {
                    serverName.Text = new Uri(monitor.Detail.WebURL).Host;
                }
                lastBuildLabel.Text = monitor.Detail.LastBuildLabel;
                lastBuildTime.Text = monitor.Detail.LastBuildTime.ToString();
                projectStatus.Text = monitor.ProjectIntegratorState;
                activity.Text = monitor.Detail.Activity.ToString();
                category.Text = monitor.Detail.Category;
                qName.Text = monitor.Detail.QueueName;
                qPriority.Text = monitor.Detail.QueuePriority.ToString("D8");

            }
            else
            {
                activity.Text = lastBuildLabel.Text = string.Empty;
            }

            detail.Text = detailStringProvider.FormatDetailString(monitor.Detail);
        }

    }
}
