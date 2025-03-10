using Tsutskiridze.TradeBuddy.DTOs.AlphaVantage;
using Tsutskiridze.TradeBuddy.DTOs.Fmp;
using Tsutskiridze.TradeBuddy.Models.AlphaVantage;
using Tsutskiridze.TradeBuddy.Models.Fmp;

namespace Tsutskiridze.TradeBuddy.Mappers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<AnnualReportDto, AnnualReport>();

            CreateMap<StockQuoteDto, StockQuote>()
                .ForMember(
                    dest => dest.Timestamp,
                    opt => opt.MapFrom(src => DateTimeOffset.FromUnixTimeSeconds(long.Parse(src.Timestamp)).DateTime)
                );
        }
    }
}
