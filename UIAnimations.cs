using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UIAnimations : MonoBehaviour
{
    public void ActivateWithBoingEffect(GameObject target, float duration = 0.5f)
    {
        if (target == null) return;
        
        // Nesneyi aktif hale getir
        target.SetActive(true);

        // Başlangıçta ölçeği sıfır yap
        target.transform.localScale = Vector3.zero;

        // Ölçeği büyütüp küçülterek boing efekti verme
        target.transform.DOScale(1.1f, duration * 0.5f)  // Önce %120 büyüt
            .SetEase(Ease.OutBack)                        // Yumuşak sıçrama efekti
            .OnComplete(() => 
                target.transform.DOScale(1f, duration * 0.3f) // Sonra %100 boyuta getir
                .SetEase(Ease.InOutBounce)
            );
    }
}
