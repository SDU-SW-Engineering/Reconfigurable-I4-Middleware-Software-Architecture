using System;
using System.Collections.Generic;
namespace Orchestrator.DomainObjects
{
    public struct Product
    {
        private bool _finished;
        private string _error;
        private DateTime _start;
        private DateTime _end;
        public string id
        {
            get => _id;
            set
            {
                _id = value;
                _start = DateTime.Now;
            }
        }

        public string orderId
        {
            get => _orderId;
            set => _orderId = value;
        }

        public List<string> logs
        {
            get => _logs??=new List<string>();
            set => _logs = value;
        }

        public bool finished
        {
            get => _finished;
            set
            {
                _finished = value;
                _end = DateTime.Now;
                OnSuccess?.Invoke(this);
            }
        }

        public DateTime startTime
        {
            get => _start;
        }
        
        public DateTime endTime
        {
            get
            {
                if (_end == DateTime.MinValue) throw new ArgumentNullException("Production of product has not stopped yet");
                return _end;
            } 
        }

        public string error
        {
            get => _error;
            set
            {
                _error = value;
                OnFailure?.Invoke(this);
            } 
        }

        public event Action<Product> OnSuccess;
        public event Action<Product> OnFailure;
        private string _id;
        private string _orderId;
        private List<string> _logs;
    }
}