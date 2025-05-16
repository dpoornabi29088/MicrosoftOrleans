using Orleans;

namespace MicrosoftOrleans.Application.DTOs;

[GenerateSerializer]
public record CreateAddressDto(string UserName, string City, string Street, string Alley, int Plaque);
