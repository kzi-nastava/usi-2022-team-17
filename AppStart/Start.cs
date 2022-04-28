﻿using HealthcareSystem.Entity.DrugModel;
using HealthcareSystem.Entity;
using HealthcareSystem.Entity.RoomModel;
using MongoDB.Driver;
using HealthcareSystem.Entity.UserModel;
using HealthcareSystem.Entity.Enumerations;
using HealthcareSystem.Entity.DoctorModel;
using HealthcareSystem.Entity.ApointmentModel;
using HealthcareSystem.Entity.CheckModel;
using HealthcareSystem.Entity.HealthCardModel;
using MongoDB.Bson;
using HealthcareSystem.Functions;
using HealthcareSystem.UI;

namespace HealthcareSystem.AppStart;



static class Start
{

    public static void ProgramStart()
    {
    

  
        var settings = MongoClientSettings.FromConnectionString("mongodb+srv://Tim17:UXGhApWVw9oc6VGg@cluster0.se6mz.mongodb.net/myFirstDatabase?retryWrites=true&w=majority");
        settings.ServerApi = new ServerApi(ServerApiVersion.V1);
        var client = new MongoClient(settings);
        var database = client.GetDatabase("USI");


        User loggedUser = null;

        while (loggedUser == null) {
            Console.WriteLine("############Hospital: Novak Djokovic####################");
            Console.Write("Enter e-mail(or x for exti): ");
            string email = Console.ReadLine();
            if(email == "x")
            {
                loggedUser = null;
                break;
            }
            Console.WriteLine();
            Console.Write("Enter password: ");
            string password = Console.ReadLine();

            loggedUser = Login.validate(database, email, password);


        }


        if (loggedUser != null)
        {
            Console.WriteLine(loggedUser.name);


            if (loggedUser.role == Role.MANAGER)
            {
                ManagerUI ui = new ManagerUI(database, loggedUser);
                loggedUser = null;
            }
        }
    }

}
