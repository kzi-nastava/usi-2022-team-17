﻿using HealthcareSystem.Entity.UserModel;
using HealthcareSystem.Entity.UserActionModel;
using HealthcareSystem.RoleControllers;
using HealthcareSystem.Entity.DoctorModel;
using HealthcareSystem.Entity.ApointmentModel;
using HealthcareSystem.Entity.Enumerations;
using HealthcareSystem.Entity.UserActionModel;
using Autofac;
using MongoDB.Driver;
using MongoDB.Bson;
using HealthcareSystem.Entity.RoomModel.RoomFiles;

namespace HealthcareSystem.UI.Patient
{
    partial class RegularScheduling : Form
    {
        public User loggedUser { get; set; }
        public UserActionService userActionService { get; set; }
        public RoomService roomService { get; set; }
        public BlockedUserService blockedUserService { get; set; }
        public DoctorService doctorService { get; set; }
        public AppointmentService appointmentService { get; set; }
        public ObjectId doctorId { get; set; }
        public RegularScheduling(User loggedUser, ObjectId doctorId)
        {
            userActionService = Globals.container.Resolve<UserActionService>();
            roomService = Globals.container.Resolve<RoomService>();
            appointmentService = Globals.container.Resolve<AppointmentService>();
            doctorService = Globals.container.Resolve<DoctorService>();
            blockedUserService = Globals.container.Resolve<BlockedUserService>();
            InitializeComponent();
            this.loggedUser = loggedUser;
            this.doctorId = doctorId;
        }

        public void setDoctor()
        {
            Doctor doctor = doctorService.GetById(doctorId);
            if (doctor != null)
            {
                string doctorName = doctor.name + " " + doctor.lastName;
                doctorBox.Items.Add(doctorName);
                doctorBox.SelectedIndex = 0;
            }
            
        }

        public void trollCheck()
        {
            int create = 0, change = 0;
            List<UserAction> userActions = userActionService.GetAllById(loggedUser._id);
            foreach (UserAction userAction in userActions)
            {
                if (DateTime.Compare(userAction.dateTime, DateTime.Today.AddMinutes(-43200)) > 0)
                {
                    if (userAction.actionStatus == ActionStatus.CREATE)
                    {
                        create = create + 1;
                    }
                    else
                    {
                        change = change + 1;
                    }
                }
            }
            if (create >= 8 || change >= 5)
            {
                BlockedUser blockedUser = new BlockedUser(loggedUser._id, BlockedBy.SYSTEM);
                blockedUserService.Insert(blockedUser);
                this.Hide();
                Console.WriteLine("The account is blocked because too much changes were made to the appointments. Contact secretary for more info.");
                System.Environment.Exit(1);
            }
        }

        private void checkAvailability()
        {
            DateTime date = datePicker.Value.Date + timePicker.Value.TimeOfDay;
            List<Doctor> allDoctors = doctorService.GetAll();
            List<Appointment> allApointments = appointmentService.GetAll();
            List<Room> allRooms = roomService.GetAll();
            List<ObjectId> unavailableRoomId = new List<ObjectId>();
            List<ObjectId> unavailableDoctorId = new List<ObjectId>();
            foreach (Appointment appointment in allApointments)
            {
                DateTime startPoint = appointment.dateTime.AddMinutes(-15);
                DateTime endPoint = appointment.dateTime.AddMinutes(15);
                if (DateTime.Compare(date, startPoint) > 0 && DateTime.Compare(date, endPoint) < 0)
                {
                    unavailableDoctorId.Add(appointment.doctorId);
                    unavailableRoomId.Add(appointment.roomId);
                }
            }
            foreach (ObjectId id in unavailableDoctorId)
            {
                foreach (Doctor doc in allDoctors.ToList())
                {
                    if (doc._id == id)
                    {
                        allDoctors.Remove(doc);
                    }
                }
            }

            int operationRooms = 0, checkupRooms = 0;
            foreach (Room room in allRooms.ToList())
            {
                if (room.type == RoomType.OPERATION_ROOM)
                {
                    operationRooms++;
                }
                else if (room.type == RoomType.CHECKUP_ROOM)
                {
                    checkupRooms++;
                }
                foreach (ObjectId id in unavailableRoomId.ToList())
                {
                    if ((room._id == id && room.type == RoomType.OPERATION_ROOM) || (room.type == RoomType.OPERATION_ROOM && room.InRenovation == true))
                    {
                        operationRooms--;
                    }
                    else if ((room._id == id && room.type == RoomType.CHECKUP_ROOM) || (room.type == RoomType.CHECKUP_ROOM && room.InRenovation == true))
                    {
                        checkupRooms--;
                    }
                }
            }
            if (allDoctors.Count == 0 || allRooms.Count == 0)
            {
                warningLabel.Text = "There are no avaialble appoitnments for the selected date.";
                warningLabel.Visible = true;
                doctorBox.Text = "";
                doctorBox.Items.Clear();
                typeBox.Text = "";
                typeBox.Items.Clear();
                submitBtn.Visible = false;
            }
            else
            {
                warningLabel.Visible = false;
                submitBtn.Visible = true;
                doctorBox.Items.Clear();
                typeBox.Items.Clear();
                foreach (Doctor doctor in allDoctors)
                {
                    string doctorName = doctor.name + " " + doctor.lastName;
                    doctorBox.Items.Add(doctorName);
                }
                if (operationRooms > 0)
                {
                    typeBox.Items.Add("Operation");
                }
                if (checkupRooms > 0)
                {
                    typeBox.Items.Add("Checkup");
                }
            }
        }
        private void RegularScheduling_Load(object sender, EventArgs e)
        {
            datePicker.MinDate = DateTime.Today;
            timePicker.CustomFormat = "HH:mm";
            timePicker.Format = DateTimePickerFormat.Custom;
            typeBox.Items.Add("Operation");
            typeBox.Items.Add("Checkup");
            setDoctor();
        }
        private void datePicker_ValueChanged(object sender, EventArgs e)
        {
            DateTime dateChosen = datePicker.Value.Date;
            if(dateChosen == DateTime.Today.Date)
            {
                timePicker.MinDate = DateTime.Now;
            }
            else
            {
                timePicker.MinDate = DateTime.Parse("00:00:00");
            }
            checkAvailability();
        }

        private void timePicker_ValueChanged(object sender, EventArgs e)
        {
            checkAvailability();
        }

        private void typeBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void submitBtn_Click(object sender, EventArgs e)
        {
            if (typeBox.SelectedIndex == -1 || doctorBox.SelectedIndex == -1)
            {
                warningLabel.Text = "The options selected are invalid. Try again.";
                warningLabel.Visible = true;
            }
            else
            {
                warningLabel.Visible = false;
                DateTime date = datePicker.Value.Date + timePicker.Value.TimeOfDay;
                List<Appointment> allApointments = appointmentService.GetAll();
                List<Room> allRooms = roomService.GetAll();
                List<ObjectId> unavailableRoomId = new List<ObjectId>();
                foreach (Appointment appointment in allApointments)
                {
                    DateTime startPoint = appointment.dateTime.AddMinutes(-15);
                    DateTime endPoint = appointment.dateTime.AddMinutes(15);
                    if (DateTime.Compare(date, startPoint) > 0 && DateTime.Compare(date, endPoint) < 0)
                    {
                        unavailableRoomId.Add(appointment.roomId);
                    }
                }
                RoomType roomType;
                ApointmentType appointmentType;
                if (typeBox.SelectedItem == "Operation")
                {
                    roomType = RoomType.OPERATION_ROOM;
                    appointmentType = ApointmentType.OPERATION;
                }
                else
                {
                    roomType = RoomType.CHECKUP_ROOM;
                    appointmentType = ApointmentType.CHECKUP;
                }
                foreach (Room room in allRooms.ToList())
                {
                    foreach (ObjectId id in unavailableRoomId.ToList())
                    {
                        if (room._id == id && room.type == roomType)
                        {
                            allRooms.Remove(room);
                        }
                    }
                    if (room.type != roomType)
                    {
                        allRooms.Remove(room);
                    }
                }
                Doctor doctor = doctorService.GetByNameAndLastName(doctorBox.SelectedItem.ToString().Split(" ")[0], doctorBox.SelectedItem.ToString().Split(" ")[1]);
                Appointment appointmentsubmit = new Appointment(date, appointmentType, doctor._id, allRooms[0]._id, loggedUser._id);
                appointmentService.Insert(appointmentsubmit);
                warningLabel.Text = "Appointment scheduled sucessfully!";
                warningLabel.Visible = true;
                UserAction userAction = new UserAction(loggedUser._id, appointmentsubmit._id, DateTime.Today, ActionStatus.CREATE);
                userActionService.Insert(userAction);
                trollCheck();
            }
        }
    }
}
