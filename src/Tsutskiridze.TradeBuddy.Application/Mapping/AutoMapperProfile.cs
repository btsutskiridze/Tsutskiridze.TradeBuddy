using AutoMapper;
using Tsutskiridze.TradeBuddy.Application.Contracts.Integrations.AlphaVantage;
using Tsutskiridze.TradeBuddy.Application.Contracts.Integrations.FinancialModelingPrep;
using Tsutskiridze.TradeBuddy.Application.Contracts.MarketData;

namespace Tsutskiridze.TradeBuddy.Application.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<AlphaVantageAnnualReportResponse, AnnualReportDto>();
            CreateMap<AlphaVantageStockOverviewResponse, StockOverviewDto>();

            CreateMap<FmpStockQuoteResponse, StockQuoteDto>()
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
