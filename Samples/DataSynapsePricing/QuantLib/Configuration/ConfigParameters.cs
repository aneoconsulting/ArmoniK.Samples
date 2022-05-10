public class PricingParameters
{
  public double Spot { get; set; }

  public double[] Input1 { get; set; }
  public double[] Input2 { get; set; }
  public double[] Input3 { get; set; }
  public double[] Input4 { get; set; }

}

public class ConfigParameters
{
  public double DefaultValue;

  public enum PricingState
  {
    AtSpot,
    AtMaturity,
    AtClosing
  }

  public PricingState State;

  public PricingParameters PricingParameters { get; set; }

}