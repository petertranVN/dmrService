using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SignalrServer.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignalrServer
{
    public class ScalingHub : Hub
    {
        DataContext _context;
        public ScalingHub(DataContext context)
        {
            _context = context;
        }

        public async Task SendMessage(string scalingMachineID, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", scalingMachineID, message);
        }
        public async Task Welcom(string scalingMachineID, string message, string unit)
        {
            await Clients.All.SendAsync("Welcom", scalingMachineID, message, unit);
        }
        public async Task Welcom2(string scalingMachineID, string message, string unit)
        {
            await Clients.All.SendAsync("Welcom2", scalingMachineID, message, unit);
        }
        public async Task Issue(string scalingMachineID, string message)
        {
            await Clients.All.SendAsync("ReceiveIssue", scalingMachineID, message);
        }
        public async Task JoinHub(int machineID)
        {
            var item = await _context.UserJoinHub.FirstOrDefaultAsync(x => x.MachineID == machineID);
            if (item == null)
            {
                await _context.AddAsync(new UserJoinHub
                {
                    ClientId = Context.ConnectionId,
                    Status = true,
                    MachineID = machineID,
                    CreatedTime = DateTime.Now
                });
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch
                {

                }
                await Clients.All.SendAsync("ReceiveJoinHub", machineID);
                // create
            }
            else
            {
                // update status
                item.ClientId = Context.ConnectionId;
                item.Status = true;
                item.CreatedTime = DateTime.Now;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch
                {

                }
                await Clients.All.SendAsync("ReceiveJoinHub", machineID);

            }
        }

        public override async Task OnConnectedAsync()
        {

            await Clients.All.SendAsync("Welcom", Context.ConnectionId, "Welcom to signal server!");

            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var item = await _context.UserJoinHub.FirstOrDefaultAsync(x => x.ClientId == Context.ConnectionId);
            if (item != null)
            {
               
                // update status
                item.ClientId = Context.ConnectionId;
                item.Status = false;
                item.CreatedTime = DateTime.Now;
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch
                {

                }
                await Clients.All.SendAsync("ReceiveJoinHub", item.MachineID);

            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
