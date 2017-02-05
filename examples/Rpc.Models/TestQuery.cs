﻿namespace Rpc.Models
{
    using DotNetty.Rpc.Protocol;

    public class TestCityQuery : AbsMessage<CityInfo>
    {
        public  int Id { get; set; }
    }

    public class CityInfo : IResult
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
