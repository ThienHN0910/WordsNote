using AutoMapper;
using WordsNote.Application.DTOs;
using WordsNote.Domain.Entities;

namespace WordsNote.Application.Mappings;

public class DeckProfile : Profile
{
    public DeckProfile()
    {
        CreateMap<Deck, DeckDto>();
    }
}
