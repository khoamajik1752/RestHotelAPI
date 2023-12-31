﻿using AutoMapper;
using Hotel.BusinessLogic.DTO.RoomDetail;
using Hotel.BusinessLogic.DTO.RoomRegulation;
using Hotel.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel.BusinessLogic.Profiles
{
    public class RoomDetailProfile: Profile
    {
        public RoomDetailProfile()
        {
            CreateMap<RoomDetail, RoomDetailToCreateDTO>();
            CreateMap<RoomDetailToCreateDTO, RoomDetail>().ForMember(des => des.Id, src => src.Ignore());
            CreateMap<RoomDetailToUpdateDTO,RoomDetailToReturnDTO>().ForMember(des=>des.roomRegulation, src => src.Ignore());
            CreateMap<RoomDetailToUpdateDTO,RoomDetail>().ForMember(des=>des.RoomRegulation, src => src.Ignore());
            CreateMap<RoomDetail, RoomDetailToReturnDTO>().ForMember(des=>des.roomRegulation,src=>src.MapFrom(x=>x.RoomRegulation) );
            CreateMap<RoomDetailToReturnDTO, RoomDetail>();
        }
    }
}
