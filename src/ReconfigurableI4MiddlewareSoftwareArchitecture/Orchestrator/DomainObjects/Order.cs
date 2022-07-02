using System;
using System.Collections.Concurrent;
using System.Linq;
using Orchestrator.Adapter.RecipeInterpretation;


namespace Orchestrator.DomainObjects
{
    public class Order
    {
        private string _orderId1;
        private DateTime _startTime;
        private DateTime _finishedTime;
        private Recipe _recipe;
        private int _totalAmount = 0;
        private ConcurrentDictionary<string, Product> _failedProducts;
        private ConcurrentDictionary<string, Product> _producedProducts;
        private ConcurrentDictionary<string, Product> _productsInProduction;
        private int counter = 0;
        
        public Order()
        {
            _failedProducts = new ConcurrentDictionary<string, Product>();
            _producedProducts = new ConcurrentDictionary<string, Product>();
            _productsInProduction = new ConcurrentDictionary<string, Product>();
            _startTime = DateTime.Now;
        }
        public string OrderId
        {
            get => _orderId1;
            set => _orderId1 = value;
        }
        
        public DateTime StartTime => _startTime;

        public DateTime FinishedTime => _finishedTime;

        public Recipe Recipe
        {
            get => _recipe;
            set => _recipe = value;
        }

        public int TotalAmount
        {
            get => _totalAmount;
            set => _totalAmount = value;
        }

        public int GetAmountInProduction()
        {
            return _productsInProduction.Count;
        }
        
        public int GetAmountProduced()
        {
            return _producedProducts.Count;
        }
        
        public string GetFailureDescription()
        {
            return string.Join(",", _failedProducts.Select(f => f.Value.error));
        }

        public void AddToProduced(Product product)
        {
            _producedProducts.TryAdd(product.id, product);
            
            Console.WriteLine("counter: " + ++counter);
            Console.WriteLine($"added: {product.id}");
            Console.WriteLine($"Now produced: {_producedProducts.Count} of {_totalAmount}" );
            if (_producedProducts?.Count == _totalAmount)
            {
                _finishedTime = DateTime.Now;
                OnSuccess?.Invoke(this);
            }
        }
        
        public void AddToInProduction(Product product)
        {
            _productsInProduction.TryAdd(product.id, product);
        }
        
        public void RemoveFromInProduction(Product product)
        {
            _productsInProduction.TryRemove(product.id, out _);
        }

        public void AddToFailed(Product product)
        {
            _failedProducts.TryAdd(product.id, product);
            OnFailure?.Invoke(this);
        }

        public event Action<Order> OnSuccess;
        public event Action<Order> OnFailure;

        public override string ToString()
        {
            return $"Order with id: {_orderId1}, amount: {string.Join(",",_totalAmount)}, produced: {string.Join(",",_producedProducts)}";
        }
    }
}