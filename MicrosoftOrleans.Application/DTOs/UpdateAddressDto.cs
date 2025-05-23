using Orleans;

namespace MicrosoftOrleans.Application.DTOs;

[GenerateSerializer]
public record UpdateAddressDto(string UserName, int addressId, string City, string Street, string Alley, int Plaque);
