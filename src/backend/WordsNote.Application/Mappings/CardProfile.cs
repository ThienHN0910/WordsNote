using AutoMapper;
using WordsNote.Application.DTOs;
using WordsNote.Domain.Entities;

namespace WordsNote.Application.Mappings;

public class CardProfile : Profile
{
    public CardProfile()
    {
        CreateMap<Card, CardDto>();
    }
}
