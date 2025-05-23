using Orleans;

namespace MicrosoftOrleans.Domain.Entities;

[GenerateSerializer]
public class OrleansStorage
{
    [Id(0)]
    public long GrainIdN0 { get; set; }
    [Id(1)]
    public long GrainIdN1 { get; set; }
    [Id(2)]
    public int GrainTypeHash { get; set; }
    [Id(3)]
    public string GrainTypeString { get; set; }
    [Id(4)]
    public string GrainIdExtensionString { get; set; }
    [Id(5)]
    public string ServiceId { get; set; }
    [Id(6)]
    public byte[] PayloadBinary { get; set; }
    [Id(7)]
    public DateTime ModifiedOn { get; set; }
    [Id(8)]
    public int Version { get; set; }
}
