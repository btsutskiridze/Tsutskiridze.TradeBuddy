using AutoMapper;
using Tsutskiridze.TradeBuddy.Application.Dtos.AlphaVantage;
using Tsutskiridze.TradeBuddy.Application.Dtos.Fmp;
using Tsutskiridze.TradeBuddy.Core.Entities;

namespace Tsutskiridze.TradeBuddy.Application.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<AnnualReportDto, AnnualReport>();

            CreateMap<StockQuoteDto, StockQuote>()
                .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src =>
                    DateTimeOffset
                        .FromUnixTimeSeconds(long.Parse(src.Timestamp))
                        .UtcDateTime
                        .ToString("yyyy-MM-ddTHH:mm:ssZ")
                ))
                .ForMember(dest => dest.EarningsAnnouncement, opt => opt.MapFrom(src =>
                    src.EarningsAnnouncement.ToString("yyyy-MM-ddTHH:mm:ssZ")
                ));
        }
    }
}
