using Orleans;

namespace MicrosoftOrleans.Application.DTOs;

[GenerateSerializer]
public record DeleteAddressDto(string UserName, int AddressId);