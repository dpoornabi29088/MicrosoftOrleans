using Orleans;

namespace MicrosoftOrleans.Application.DTOs;

[GenerateSerializer]
public record DeleteAddressDto(string UserName, string City, string Street, string Alley, int Plaque);