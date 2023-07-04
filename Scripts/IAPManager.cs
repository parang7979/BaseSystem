#if USING_UNITY_IAP
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace Parang
{
    public class IAPManager : Singleton<IAPManager>, IDetailedStoreListener
    {
        private IStoreController _storeController;
        private IExtensionProvider _extensionProvider;
        private UnityEngine.Purchasing.Product _pendingProduct;

        public void Init()
        {
            var instance = StandardPurchasingModule.Instance();
            var builder = ConfigurationBuilder.Instance(instance);
            builder.AddProduct("Test", ProductType.Consumable);
            UnityPurchasing.Initialize(this, builder);
        }

        public void Purchase(string productId)
        {
            var product = _storeController.products.WithID(productId);
            if (product != null && product.availableToPurchase)
            {
                _storeController.InitiatePurchase(product);
            }
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            Debug.Log("IAP initialized");
            foreach(var p in controller.products.all)
                Debug.Log(p);
            _storeController = controller;
            _extensionProvider = extensions;
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.Log($"IAP initialized Failed - {error}");
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Debug.Log($"IAP initialized failed - {error}, {message}");
        }

        public void OnPurchaseFailed(UnityEngine.Purchasing.Product product, PurchaseFailureDescription failureDescription)
        {
            Debug.Log($"IAP purchase failed - {product}, {failureDescription}");
        }

        public void OnPurchaseFailed(UnityEngine.Purchasing.Product product, PurchaseFailureReason failureReason)
        {
            Debug.Log($"IAP purchase failed - {product}, {failureReason}");
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            Debug.Log(purchaseEvent.purchasedProduct.receipt);

            /* _pendingProduct = purchaseEvent.purchasedProduct;
            return PurchaseProcessingResult.Pending; */

            return PurchaseProcessingResult.Complete;
        }
    }
}
#endif