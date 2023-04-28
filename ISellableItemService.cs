namespace Integrations.HKTDC.SPCS.Services;

/// <summary>
/// SPSC item contract.
/// </summary>
public interface ISellableItemService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="changedSince"></param>
    /// <returns></returns>
    Task<IEnumerable<SellableItem>> GetItemsAsync(DateTime changedSince);
    Task ImportSellableItem(SellableItem sellableItem);

    /* Get all the items that have changed
     * For each item
     *  For each tier
     *     Create the resource
     *     Create a price list entry for HKD
     *     Create a price list entry for USD
     *     Need a mapping table to determine the revenue event type
     */
}
