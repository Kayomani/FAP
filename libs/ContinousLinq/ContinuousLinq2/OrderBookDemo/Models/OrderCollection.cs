using System.Collections.ObjectModel;

namespace ContinuousLinq.OrderBookDemo.Models
{
	public class OrderCollection : ObservableCollection<Order>
	{
		public OrderCollection()
		{
			Populate();
		}

		private void Populate()
		{
			for (int i = 0; i < 25; i++)
			{
				Order o = new Order()
				          	{
				          		Ask = 12.33,
								Bid = 12.45,
								Price = 12.56
				          	};

				Add(o);
			}
		}
	}
}