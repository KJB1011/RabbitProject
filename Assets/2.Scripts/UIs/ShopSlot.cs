using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using static Defines;

/// <summary>
/// 상점에 있는 아이템 슬롯(버튼)을 담당하는 스크립트
/// </summary>
public class ShopSlot : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI 참조")]
    [SerializeField] Image _itemImage;
    [SerializeField] TextMeshProUGUI _priceText;
    [SerializeField] RectTransform _imageRect;   // 호버 시 커질 이미지
    [SerializeField] GameObject _descPanel;      // 설명창 루트
    [SerializeField] TextMeshProUGUI _nameText;  // 이름 텍스트
    [SerializeField] TextMeshProUGUI _descText;  // 설명 텍스트

    [Header("호버 설정")]
    [SerializeField] float _hoverScale = 1.15f;
    [SerializeField] float _hoverDuration = 0.15f;

    // 슬롯 유형
    public enum SlotType { Artifact, FixedItem }

    SlotType _slotType;
    ArtifactSO _artifactData;   // 아티팩트 슬롯용
    ITEM _fixedItem;            // 고정 아이템 슬롯용
    int _price;
    bool _sold = false;
    Vector3 _originalScale;
    bool _showDesc = false;

    Action _onPurchased;        // 구매 완료 시 ShopUI에 알림

    // ── 슬롯 초기화 ───────────────────────────────────────────

    /// <summary>아티팩트 슬롯 초기화</summary>
    public void SetupArtifact(ArtifactSO data, Action onPurchased)
    {
        _showDesc = true;

        _slotType = SlotType.Artifact;
        _artifactData = data;
        _onPurchased = onPurchased;
        _price = data.price;
        _sold = false;

        _itemImage.sprite = data.icon;
        _itemImage.color = Color.white;
        _priceText.text = data.price.ToString();

        if (_descText != null) _nameText.text = data.artifactName;
        if (_descText != null) _descText.text = data.description;
        if (_descPanel != null) _descPanel.SetActive(false);

        gameObject.SetActive(true);
    }

    /// <summary>고정 아이템 슬롯 초기화</summary>
    public void SetupFixedItem(ITEM item, Sprite icon, string desc, int price,
                                System.Action onPurchased = null)
    {
        _slotType = SlotType.FixedItem;
        _fixedItem = item;
        _price = price;
        _onPurchased = onPurchased; // ← 추가
        _sold = false;

        _itemImage.sprite = icon;
        _itemImage.color = Color.white;
        _priceText.text = price.ToString();

        if (_descText != null) _descText.text = desc;
        if (_descPanel != null) _descPanel.SetActive(false);

        gameObject.SetActive(true);
    }

    void Awake()
    {
        _originalScale = _imageRect != null
            ? _imageRect.localScale
            : Vector3.one;
    }

    // ── 호버 ─────────────────────────────────────────────────

    public void OnPointerEnter(PointerEventData _)
    {
        if (_imageRect != null)
            _imageRect.DOScale(_originalScale * _hoverScale, _hoverDuration)
                      .SetEase(Ease.OutBack);

        // _showDesc가 true일 때만 설명창 표시
        if (_showDesc && _descPanel != null)
            _descPanel.SetActive(true);
    }

    public void OnPointerExit(PointerEventData _)
    {
        if (_imageRect != null)
            _imageRect.DOScale(_originalScale, _hoverDuration)
                      .SetEase(Ease.InBack);

        if (_descPanel != null)
            _descPanel.SetActive(false);
    }

    // ── 클릭 (구매) ──────────────────────────────────────────

    public void OnPointerClick(PointerEventData _)
    {
        if (_sold) return;

        // 골드 확인
        if (IngameManager.Instance.Gold < _price)
        {
            ShowFloatingText("자금 부족!", Color.red);
            return;
        }

        // 구매 처리
        IngameManager.Instance.SpendGold(_price);

        switch (_slotType)
        {
            case SlotType.Artifact:
                ArtifactManager.Instance.Acquire(_artifactData.id);
                // 아티팩트는 구매 후 슬롯 숨기기
                _sold = true;
                _itemImage.color = new Color(1f, 1f, 1f, 0.3f);
                _priceText.text = "구매완료";
                break;

            case SlotType.FixedItem:
                ApplyFixedItem();
                // 소모품은 계속 구매 가능
                break;
        }

        ShowFloatingText("구매완료!", new Color(0.3f, 1f, 0.3f));
        _onPurchased?.Invoke();
    }

    private void ApplyFixedItem()
    {
        var player = FindFirstObjectByType<PlayerController>();
        if (player == null) return;

        switch (_fixedItem)
        {
            case ITEM.HP: player.RecoverHp(1); break;
            case ITEM.ATKUP: player.AddAtk(1f); break;
            case ITEM.CRITRATEUP: player.AddCritRate(1f); break;
        }
    }

    // ── FloatingText ─────────────────────────────────────────

    private void ShowFloatingText(string content, Color color)
    {
        FloatingTextManager.Instance.ShowCustom(content, color, 2.5f, GetWorldPos());
    }

    private Vector3 GetWorldPos()
    {
        Vector3 screenPos = transform.position;
        float depth = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 world = Camera.main.ScreenToWorldPoint(
            new Vector3(screenPos.x, screenPos.y, depth));
        world.z = 0f;
        return world;
    }
}