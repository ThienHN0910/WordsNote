using AutoMapper;
using WordsNote.Application.DTOs;
using WordsNote.Domain.Entities;

namespace WordsNote.Application.Mappings;

public class DeckProfile : Profile
{
    public DeckProfile()
    {
        CreateMap<Deck, DeckDto>()
            .ForMember(dest => dest.CardCount, opt => opt.MapFrom(src => src.Cards.Count))
            .ForMember(dest => dest.DueCardCount, opt => opt.MapFrom(src =>
                src.Cards.Count(c => c.NextReviewDate <= DateTime.UtcNow)));
    }
}
