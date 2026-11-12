using AutoMapper;
using Domain.DTOs.BookingDTOs;
using Domain.DTOs.MatchDTOs;
using Domain.DTOs.MatchRequestDTOs;
using Domain.DTOs.PaymentDTOs;
using Domain.DTOs.ReviewDTOs;
using Domain.DTOs.StadiumDTOs;
using Domain.DTOs.StadiumPhotoDTOs;
using Domain.DTOs.UserDTOs;
using Domain.Entities;

namespace Infrastructure.AutoMapper;

public class InfrastructureProfile : Profile
{
    public InfrastructureProfile()
    {
        // ===== STADIUM =====
        CreateMap<CreateStadiumDTOs, Stadium>();

        CreateMap<UpdateStadiumDTOs, Stadium>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Stadium, GetStadiumDTOs>()
            .ForMember(dest => dest.OwnerFullName,
                opt => opt.MapFrom(src => src.Owner != null ? src.Owner.FullName : string.Empty))
            .ForMember(dest => dest.AverageRating,
                opt => opt.MapFrom(src => src.Reviews.Any()
                    ? src.Reviews.Average(r => r.Rating)
                    : 0.0))
            .ForMember(dest => dest.TotalReviews,
                opt => opt.MapFrom(src => src.Reviews.Count))
            .ForMember(dest => dest.PhotoUrls,
                opt => opt.MapFrom(src => src.Photos.Select(p => p.PhotoUrl).ToList()));

        // ===== STADIUM PHOTO =====
        CreateMap<CreateStadiumPhotoDTOs, StadiumPhoto>();
        CreateMap<StadiumPhoto, GetStadiumPhotoDTOs>();

        // ===== BOOKING =====
        CreateMap<CreateBookingDTOs, Booking>();

        CreateMap<UpdateBookingDTOs, Booking>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Booking, GetBookingDTOs>()
            .ForMember(dest => dest.StadiumName,
                opt => opt.MapFrom(src => src.Stadium != null ? src.Stadium.Name : string.Empty))
            .ForMember(dest => dest.UserFullName,
                opt => opt.MapFrom(src => src.User != null ? src.User.FullName : string.Empty));

        // ===== REVIEW =====
        CreateMap<CreateReviewDTOs, Review>();

        CreateMap<UpdateReviewDTOs, Review>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Review, GetReviewDTOs>()
            .ForMember(dest => dest.StadiumName,
                opt => opt.MapFrom(src => src.Stadium != null ? src.Stadium.Name : string.Empty))
            .ForMember(dest => dest.UserFullName,
                opt => opt.MapFrom(src => src.User != null ? src.User.FullName : string.Empty));

        // ===== PAYMENT =====
        CreateMap<CreatePaymentDTOs, Payment>();

        CreateMap<UpdatePaymentDTOs, Payment>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Payment, GetPaymentDTOs>()
            .ForMember(dest => dest.UserFullName,
                opt => opt.MapFrom(src => src.User != null ? src.User.FullName : string.Empty));

        // ===== MATCH =====
        CreateMap<CreateMatchDTOs, Match>();

        CreateMap<UpdateMatchDTOs, Match>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Match, GetMatchDTOs>()
            .ForMember(dest => dest.StadiumName,
                opt => opt.MapFrom(src => src.Stadium != null ? src.Stadium.Name : string.Empty))
            .ForMember(dest => dest.CreatorFullName,
                opt => opt.MapFrom(src => src.Creator != null ? src.Creator.FullName : string.Empty));

        // ===== MATCH REQUEST =====
        CreateMap<CreateMatchRequestDTOs, MatchRequest>();

        CreateMap<UpdateMatchRequestDTOs, MatchRequest>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<MatchRequest, GetMatchRequestDTOs>()
            .ForMember(dest => dest.MatchTime,
                opt => opt.MapFrom(src => src.Match != null ? src.Match.MatchTime : default))
            .ForMember(dest => dest.UserFullName,
                opt => opt.MapFrom(src => src.User != null ? src.User.FullName : string.Empty));

        // ===== USER =====
        CreateMap<UpdateUserDTO, ApplicationUser>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<ApplicationUser, GetUserDTO>()
            .ForMember(dest => dest.Role, opt => opt.Ignore()); // роль грузится отдельно через UserManager
    }
}