using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Timers;

namespace ServiceManager
{
    // Summary:
    //     Indicates the current state of the service.
    public enum ServiceStatus
    {
        // Summary:
        //     The service is not running. This corresponds to the Win32 SERVICE_STOPPED
        //     constant, which is defined as 0x00000001.
        Stopped = 1,
        //
        // Summary:
        //     The service is starting. This corresponds to the Win32 SERVICE_START_PENDING
        //     constant, which is defined as 0x00000002.
        StartPending = 2,
        //
        // Summary:
        //     The service is stopping. This corresponds to the Win32 SERVICE_STOP_PENDING
        //     constant, which is defined as 0x00000003.
        StopPending = 3,
        //
        // Summary:
        //     The service is running. This corresponds to the Win32 SERVICE_RUNNING constant,
        //     which is defined as 0x00000004.
        Running = 4,
        //
        // Summary:
        //     The service continue is pending. This corresponds to the Win32 SERVICE_CONTINUE_PENDING
        //     constant, which is defined as 0x00000005.
        ContinuePending = 5,
        //
        // Summary:
        //     The service pause is pending. This corresponds to the Win32 SERVICE_PAUSE_PENDING
        //     constant, which is defined as 0x00000006.
        PausePending = 6,
        //
        // Summary:
        //     The service is paused. This corresponds to the Win32 SERVICE_PAUSED constant,
        //     which is defined as 0x00000007.
        Paused = 7,
    }

    public class ServiceHelper
    {
        private ServiceController _serviceController;
        public ServiceHelper(string serviceName)
        {
            _serviceController = new ServiceController(serviceName);
        }

        private Timer _eventTimer;

        public void Start()
        {
            if (_serviceController.Status != ServiceControllerStatus.Running)
            {
                _serviceController.Start();
            }

            _eventTimer = new Timer(TimeSpan.FromSeconds(10).TotalMilliseconds);
            _eventTimer.Elapsed += (sender, args) =>
                {
                    var currentStatus = _serviceController.Status;
                    _serviceController.Refresh();
                    if (currentStatus == _serviceController.Status)
                    {
                        return;
                    }
                    switch (_serviceController.Status)
                    {
                        case ServiceControllerStatus.StartPending:
                            OnServiceStatusChanged(ServiceStatus.StartPending, "正在启动");
                            break;
                        case ServiceControllerStatus.Running:
                            OnServiceStatusChanged(ServiceStatus.Running, "已启动");
                            break;
                        case ServiceControllerStatus.Paused:
                            OnServiceStatusChanged(ServiceStatus.Paused, "已暂停");
                            break;
                        case ServiceControllerStatus.StopPending:
                            OnServiceStatusChanged(ServiceStatus.StopPending, "正在停止");
                            break;
                        case ServiceControllerStatus.Stopped:
                            OnServiceStatusChanged(ServiceStatus.Stopped, "已停止");
                            break;
                    }
                };

            _eventTimer.Start();
        }

        public void Stop()
        {
            if (_serviceController.Status != ServiceControllerStatus.Stopped)
            {
                _serviceController.Stop();
            }

            _eventTimer.Stop();
        }
        public event Action<ServiceStatus, object> ServiceStatusChanged;

        protected virtual void OnServiceStatusChanged(ServiceStatus arg1, object arg2)
        {
            Action<ServiceStatus, object> handler = ServiceStatusChanged;
            if (handler != null) handler(arg1, arg2);
        }
    }
}
