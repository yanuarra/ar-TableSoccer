using System;
using UnityEngine;
using UnityEngine.UI;

namespace YRA
{
    public class EnergySystem : MonoBehaviour
    {
        public bool isPlayer;
        public bool canSpawn {get; private set;}
        private float _maxEnergy = StaticData.ENERGY_MAX;
        private float _startingEnergy = 0;
        private float _rechargeRate = StaticData.ENERGY_REGENERATION_SPEED;
        private float _spawnEnergyCost = StaticData.SPAWN_COST;
        private float _consumptionMultiplier = 1.0f;
        private float _currentEnergy;
        [SerializeField] private Slider _energySlider;
        private Image _fillImage;
        public event Action<float> OnEnergyChanged;
        public event Action OnEnergyDepleted;
        public event Action OnEnergyFull;

        
        private void Start()
        {
            _currentEnergy = Mathf.Clamp(_startingEnergy, 0, _maxEnergy);
            OnEnergyChanged?.Invoke(_currentEnergy);
            if (_currentEnergy >= _maxEnergy)
            {
                OnEnergyFull?.Invoke();
            }
            else if (_currentEnergy <= 0)
            {
                OnEnergyDepleted?.Invoke();
            }
        }

        public bool CanAffordSpawn()
        {
            return _currentEnergy >= (_spawnEnergyCost * _consumptionMultiplier);
        }


        private void Update()
        {
            // Handle energy recharge
            if (_currentEnergy < _maxEnergy)
            {
                float previousEnergy = _currentEnergy;
                _currentEnergy = Mathf.Min(_currentEnergy + (_rechargeRate * Time.deltaTime), _maxEnergy);
                if (previousEnergy != _currentEnergy)
                {
                    UpdateUI();
                    OnEnergyChanged?.Invoke(_currentEnergy);
                    if (_currentEnergy >= _maxEnergy)
                    {
                        OnEnergyFull?.Invoke();
                    }
                }
            }
        }

        private void UpdateUI()
        {
            if (_energySlider != null)
            {
                _energySlider.maxValue = _maxEnergy;
                _energySlider.value = _currentEnergy;
            }
        }

        public bool UseEnergy()
        {
            float adjustedAmount = _spawnEnergyCost * _consumptionMultiplier;
            
            // Check if we have enough energy
            if (_currentEnergy >= adjustedAmount)
            {
                float previousEnergy = _currentEnergy;
                _currentEnergy = Mathf.Max(_currentEnergy - adjustedAmount, 0);
                UpdateUI();
                OnEnergyChanged?.Invoke(_currentEnergy);
                if (previousEnergy > 0 && _currentEnergy <= 0)
                {
                    OnEnergyDepleted?.Invoke();
                }
                return true;
            }
            return false;
        }
    }
}
