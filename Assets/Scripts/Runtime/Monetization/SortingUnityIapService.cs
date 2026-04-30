#if UNITY_PURCHASING
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

//   2) UnityPurchasing.Initialize(this, builder)
public sealed class SortingUnityIapService : ISortingIapService, IDetailedStoreListener
{
    private const string OwnedKeyPrefix = "SortingPuzzle_IapOwned_";

    private IStoreController storeController;
    private IExtensionProvider extensionProvider;
    private readonly Dictionary<string, SortingIapProductDef> products = new Dictionary<string, SortingIapProductDef>();
    private Action<bool> onReadyCallback;
    private Action<string> pendingPurchaseSuccess;
    private Action<string, string> pendingPurchaseFailure;
    private string pendingPurchaseSku;

    public bool IsReady { get; private set; }

    public void Initialize(IReadOnlyList<SortingIapProductDef> productList, Action<bool> onReady)
    {
        products.Clear();
        for (int i = 0; i < productList.Count; i++)
        {
            products[productList[i].sku] = productList[i];
        }
        onReadyCallback = onReady;

        ConfigurationBuilder builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        for (int i = 0; i < productList.Count; i++)
        {
            SortingIapProductDef def = productList[i];
            ProductType type;
            switch (def.kind)
            {
                case SortingIapKind.Consumable: type = ProductType.Consumable; break;
                case SortingIapKind.NonConsumable: type = ProductType.NonConsumable; break;
                case SortingIapKind.StarterPack:
                    type = ProductType.NonConsumable;
                    break;
                default: type = ProductType.Consumable; break;
            }
            builder.AddProduct(def.sku, type);
        }

        UnityPurchasing.Initialize(this, builder);
    }

    public bool IsOwned(string sku)
    {
        if (storeController != null)
        {
            Product p = storeController.products.WithID(sku);
            if (p != null && p.hasReceipt) return true;
        }
        return PlayerPrefs.GetInt(OwnedKeyPrefix + sku, 0) == 1;
    }

    public string GetLocalizedPrice(string sku)
    {
        if (storeController != null)
        {
            Product p = storeController.products.WithID(sku);
            if (p != null && p.availableToPurchase)
            {
                return p.metadata.localizedPriceString;
            }
        }
        return products.TryGetValue(sku, out var def) ? def.fallbackPriceText : "--";
    }

    public void Purchase(string sku, Action<string> onPurchased, Action<string, string> onFailed)
    {
        if (!IsReady || storeController == null)
        {
            onFailed?.Invoke(sku, "not_ready");
            return;
        }
        Product product = storeController.products.WithID(sku);
        if (product == null || !product.availableToPurchase)
        {
            onFailed?.Invoke(sku, "unavailable");
            return;
        }

        pendingPurchaseSku = sku;
        pendingPurchaseSuccess = onPurchased;
        pendingPurchaseFailure = onFailed;
        storeController.InitiatePurchase(product);
    }

    public void RestorePurchases(Action<bool> onCompleted)
    {
        if (extensionProvider == null)
        {
            onCompleted?.Invoke(false);
            return;
        }

#if UNITY_IOS
        extensionProvider.GetExtension<IAppleExtensions>().RestoreTransactions((success, error) => onCompleted?.Invoke(success));
#elif UNITY_ANDROID
        onCompleted?.Invoke(true);
#else
        onCompleted?.Invoke(false);
#endif
    }

    // =================== IStoreListener ===================

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        storeController = controller;
        extensionProvider = extensions;
        IsReady = true;
        onReadyCallback?.Invoke(true);
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        OnInitializeFailed(error, null);
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogWarning($"[IAP] Initialize failed: {error} {message}");
        IsReady = false;
        onReadyCallback?.Invoke(false);
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        string sku = args.purchasedProduct.definition.id;
        if (products.TryGetValue(sku, out var def) && def.kind != SortingIapKind.Consumable)
        {
            PlayerPrefs.SetInt(OwnedKeyPrefix + sku, 1);
            PlayerPrefs.Save();
        }

        if (pendingPurchaseSku == sku)
        {
            pendingPurchaseSuccess?.Invoke(sku);
            ClearPendingPurchase();
        }
        else
        {
            pendingPurchaseSuccess?.Invoke(sku);
        }
        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        HandlePurchaseFailure(product?.definition?.id, failureReason.ToString());
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failure)
    {
        HandlePurchaseFailure(product?.definition?.id, failure?.reason.ToString());
    }

    private void HandlePurchaseFailure(string sku, string reason)
    {
        Debug.LogWarning($"[IAP] Purchase failed sku={sku} reason={reason}");
        if (pendingPurchaseSku == sku)
        {
            pendingPurchaseFailure?.Invoke(sku, reason);
            ClearPendingPurchase();
        }
    }

    private void ClearPendingPurchase()
    {
        pendingPurchaseSku = null;
        pendingPurchaseSuccess = null;
        pendingPurchaseFailure = null;
    }
}
#endif
