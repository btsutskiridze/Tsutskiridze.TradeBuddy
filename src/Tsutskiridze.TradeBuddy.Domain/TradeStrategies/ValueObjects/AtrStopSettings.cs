using SharedKernel;

namespace Tsutskiridze.TradeBuddy.Domain.TradeStrategies.ValueObjects;

public record AtrStopSettings : ValueObject
{
    public int Period { get; }
    public decimal InitialStopMultiplier { get; }
    public decimal TrailingStopMultiplier { get; }
    public decimal TrailingActivationMultiplier { get; }

    private AtrStopSettings(){}
    
    public AtrStopSettings(int period, decimal initialStopMultiplier, decimal trailingStopMultiplier, int trailingActivationMultiplier)
    {
        if(period <= 0)
            throw new DomainException("Period must be positive");
        
        if(initialStopMultiplier <= 0)
            throw new DomainException("Initial stop multiplier must be positive");
        if(trailingStopMultiplier <= 0)
            throw new DomainException("Trailing stop multiplier must be positive");
        
        if(trailingActivationMultiplier <= 0)
            throw new DomainException("Trailing activation multiplier must be positive");
        
        Period = period;
        InitialStopMultiplier = initialStopMultiplier;
        TrailingStopMultiplier = trailingStopMultiplier;
        TrailingActivationMultiplier = trailingActivationMultiplier;       
    }
}