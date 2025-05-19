using System.Collections.Generic;
using System;

namespace TRavljen.UnitSystem
{

    /// <summary>
    /// Delegate that allows implementation to prevent production of certain units
    /// when they reached the very end of production.
    /// <para>
    /// Example: This may be used if some
    /// resources like population needs to be above 0 before spawning/producing
    /// a unit is possible.
    /// </para>
    /// </summary>
    public interface IProductionQueueDelegate
    {
        /// <summary>
        /// Method invoked before producing the latest item. Implement this to
        /// return 'false' and prevent production from finishing. This will then
        /// be attempted for each <see cref="ProductionQueue.Produce(float)"/>
        /// iteration.
        /// </summary>
        /// <param name="producibleQuantity">Producible and its quantity</param>
        /// <returns>
        /// Return 'true' to finish production, return 'false' to prevent it.
        /// </returns>
        bool ShouldFinishProductionFor(ProducibleQuantity producibleQuantity);
    }

    /// <summary>
    /// Class used for producing items from a list, one by one.
    /// First item in queue is producing, the rest wait.
    /// It supports starting, queueing or canceling production.
    /// </summary>
    public class ProductionQueue
    {

        /// <summary>
        /// Current time of the production time for first producible in the <see cref="Queue"/>.
        /// </summary>
        private float productionTimeRemaining = -1f;
        private float productionDuration = 0;

        private readonly List<ProducibleQuantity> queue = new List<ProducibleQuantity>();

        /// <summary>
        /// Collection of the current production items. First one in queue is
        /// currently in production.
        /// </summary>
        public ProducibleQuantity[] Queue => queue.ToArray();

        /// <summary>
        /// Gets the current queue size.
        /// </summary>
        public int QueueSize => queue.Count;

        /// <summary>
        /// Action invoked once a producible finishes production.
        /// </summary>
        public Action<ProducibleQuantity> OnProductionFinished;

        /// <summary>
        /// Returns range from 0 to 1 when an item is in production.
        /// When there is no production -1 is returned.
        /// </summary>
        public float CurrentProductionProgress { private set; get; } = -1f;

        /// <summary>
        /// Optional delegate that allows pausing of ongoing production.
        /// </summary>
        public IProductionQueueDelegate Delegate;

        /// <summary>
        /// Applies delta changes to current production queue. Only a single
        /// item can be produced at a time (first one in queue). If delta is
        /// larger than the remaining time of the current production, remainder
        /// is applied to the next production item (if exists).
        /// </summary>
        /// <param name="delta">Value applied to the production time. This
        /// can be either an actual time or time period value.</param>
        public void Produce(float delta)
        {
            if (queue.Count == 0)
            {
                productionTimeRemaining = -1;
                CurrentProductionProgress = -1;
                return;
            }

            var originalTimeRemaining = productionTimeRemaining;
            productionTimeRemaining -= delta;

            // In case its actually 0 still produce for one frame.
            if (productionTimeRemaining < 0)
            {
                var item = queue[0];

                if (Delegate != null && !Delegate.ShouldFinishProductionFor(item))
                {
                    productionTimeRemaining = originalTimeRemaining;
                    return;
                }

                float remaningTime = Math.Abs(productionTimeRemaining);
                queue.RemoveAt(0);
                OnProductionFinished?.Invoke(item);

                DequeueNextItem();

                Produce(remaningTime);
            }
            else
            {
                CurrentProductionProgress = 1f - (productionTimeRemaining / productionDuration);
            }
        }

        /// <summary>
        /// Setup first item in queue for production.
        /// </summary>
        private void DequeueNextItem()
        {
            if (queue.Count > 0)
            {
                productionDuration = queue[0].Producible.ProductionDuration;
                productionTimeRemaining = productionDuration;
                CurrentProductionProgress = 0f;
            }
            else
            {
                CurrentProductionProgress = -1;
            }
        }

        /// <summary>
        /// Checks if the production queue contains this producible.
        /// Matching is done with <see cref="ManagedSO.ID"/>.
        /// </summary>
        /// <param name="producible">Producible used for matching.</param>
        /// <returns>Returns 'true' only if this item is in production.</returns>
        public bool IsProducing(AProducibleSO producible)
        {
            for (int index = 0; index < queue.Count; index++)
            {
                if (queue[index].Producible.ID == producible.ID)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Adds a producible and its quantity to the end of production queue.
        /// </summary>
        /// <param name="producible">Producible to be added.</param>
        /// <param name="quantity">Quantity to be produced.</param>
        /// <param name="queueMultipleOrders">
        /// If quantity should be split into multiple orders, diving by quantity 1.
        /// </param>
        public void AddProductionOrder(AProducibleSO producible, long quantity, bool queueMultipleOrders = false)
        {
            // Nothing to do
            if (quantity <= 0) return;

            if (queueMultipleOrders)
            {
                while (quantity > 0)
                {
                    // Queue producible once for every 1 full production, if
                    // there are leftovers they will be queued as well.
                    // Generally full numbers are used for production, like 5 grapes
                    // but if game type desires decimals this is also supported by
                    // the leftover.
                    if (quantity > 1) queue.Add(new ProducibleQuantity(producible, 1));
                    else queue.Add(new ProducibleQuantity(producible, quantity));
                    quantity--;
                }
            }
            else
            {
                queue.Add(new ProducibleQuantity(producible, quantity));
            }

            // If this is first item, dequeue right away
            if (queue.Count == 1)
            {
                DequeueNextItem();
            }
        }

        /// <summary>
        /// Cancels the first producible that matches the parameter.
        /// </summary>
        /// <param name="producible">
        /// Producible that will be canceled if present in queue.
        /// </param>
        /// <returns>Returns quantity of the producible order cancelled.</returns>
        public long CancelProductionOrder(AProducibleSO producible)
        {
            return CancelProductionOrder(producible.ID);
        }

        /// <summary>
        /// Cancel the first producible with ID that matches the argument.
        /// </summary>
        /// <param name="producibleID">
        /// ID that will be used for finding a matching producible.
        /// </param>
        /// <returns>Returns quantity of the producible order cancelled.</returns>
        public long CancelProductionOrder(int producibleID)
        {
            for (int index = 0; index < queue.Count; index++)
            {
                if (queue[index].Producible.ID == producibleID)
                {
                    return CancelProductionOrderIndex(index);
                }
            }

            return 0;
        }

        /// <summary>
        /// Cancels the producible on specified index.
        /// </summary>
        /// <param name="index">
        /// Index on which the producible should be canceled.
        /// </param>
        /// <returns>Returns quantity of the producible order cancelled.</returns>
        public long CancelProductionOrderIndex(int index)
        {
            // Check if index is too high or too low
            if (index >= queue.Count || index < 0) return 0;

            long quantity = queue[index].Quantity;

            queue.RemoveAt(index);

            // If the first item in list was canceled, dequeue next item
            if (index == 0)
            {
                productionTimeRemaining = -1;
                DequeueNextItem();
            }

            return quantity;
        }

        /// <summary>
        /// Cancels all production orders in the queue.
        /// Returns all the accumulating resources freed duo to cancalled productions.
        /// </summary>
        public ResourceQuantity[] CancelProduction()
        {
            Dictionary<ResourceSO, long> resourcesToRefund = new();

            // Accumulate the total resource costs from each item in the production queue
            foreach (var item in queue)
            {
                var producible = item.Producible;
                long producibleQuantity = item.Quantity;

                // Accumulate the costs of each resource within the producible’s cost
                foreach (var resourceAmount in producible.Cost)
                {
                    long totalAmount = resourceAmount.Quantity * producibleQuantity;
                    resourcesToRefund.TryGetValue(resourceAmount.Resource, out long currentQuantity);
                    resourcesToRefund[resourceAmount.Resource] = currentQuantity + totalAmount;
                }
            }

            List<ResourceQuantity> resources = new();
            foreach (var refund in resourcesToRefund)
                resources.Add(new(refund.Key, refund.Value));

            queue.Clear();

            productionTimeRemaining = -1;
            CurrentProductionProgress = -1;

            return resources.ToArray();
        }
    }

}