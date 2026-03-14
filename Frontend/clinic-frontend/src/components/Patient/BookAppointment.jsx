import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { doctorAPI, patientAPI, specializationsAPI } from '../../services/api';
import toast from 'react-hot-toast';

const BookAppointment = () => {
  const navigate = useNavigate();

  const [specializations, setSpecializations] = useState([]);
  const [doctors, setDoctors]   = useState([]);
  const [slots, setSlots]       = useState([]);
  const [loading, setLoading]   = useState(false);
  const [loadingSlots, setLoadingSlots] = useState(false);

  const [form, setForm] = useState({
    specializationId: '',
    doctorId:         '',
    appointmentDate:  '',
    appointmentTime:  '',
    reasonForVisit:   '',
  });

  const today = new Date().toISOString().split('T')[0];

  // Load specializations once
  useEffect(() => {
    specializationsAPI.getAll()
      .then(res => setSpecializations(res.data.data || []))
      .catch(() => toast.error('Could not load specializations'));
  }, []);

  // Load doctors when specialization changes
  useEffect(() => {
    if (!form.specializationId) { setDoctors([]); return; }
    doctorAPI.getAll({ isAvailable: true, specializationId: form.specializationId })
      .then(res => setDoctors(res.data.data || []))
      .catch(() => toast.error('Could not load doctors'));
    setForm(prev => ({ ...prev, doctorId: '', appointmentDate: '', appointmentTime: '' }));
    setSlots([]);
  }, [form.specializationId]);

  // Load slots when doctor + date changes
  useEffect(() => {
    if (!form.doctorId || !form.appointmentDate) { setSlots([]); return; }
    setLoadingSlots(true);
    setForm(prev => ({ ...prev, appointmentTime: '' }));
    doctorAPI.getAvailableSlots(form.doctorId, form.appointmentDate)
      .then(res => setSlots(res.data.data?.availableSlots || []))
      .catch(() => toast.error('Could not load available slots'))
      .finally(() => setLoadingSlots(false));
  }, [form.doctorId, form.appointmentDate]);

  const handleChange = (e) =>
    setForm(prev => ({ ...prev, [e.target.name]: e.target.value }));

  const selectedDoctor = doctors.find(d => d.doctorId === parseInt(form.doctorId));

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!form.appointmentTime) { toast.error('Please select a time slot'); return; }
    setLoading(true);
    try {
      await patientAPI.bookAppointment({
        doctorId:       parseInt(form.doctorId),
        appointmentDate: form.appointmentDate,
        appointmentTime: form.appointmentTime + ':00',
        reasonForVisit:  form.reasonForVisit,
      });
      toast.success('Appointment booked successfully!');
      navigate('/patient/appointments');
    } catch (err) {
      toast.error(err.response?.data?.message || 'Booking failed. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <div className="page-header">
        <div>
          <h1>Book an Appointment</h1>
          <p className="text-muted">Choose a doctor and time slot</p>
        </div>
      </div>

      <div className="form-card">
        <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '1.25rem' }}>

          {/* Step 1 — Specialization */}
          <div className="form-group">
            <label>Step 1: Select Specialization</label>
            <select name="specializationId" value={form.specializationId} onChange={handleChange} required>
              <option value="">— Choose a specialization —</option>
              {specializations.map(s => (
                <option key={s.specializationId} value={s.specializationId}>{s.name}</option>
              ))}
            </select>
          </div>

          {/* Step 2 — Doctor */}
          {form.specializationId && (
            <div className="form-group">
              <label>Step 2: Select Doctor</label>
              {doctors.length === 0
                ? <p className="text-warning">No available doctors for this specialization.</p>
                : (
                  <>
                    <select name="doctorId" value={form.doctorId} onChange={handleChange} required>
                      <option value="">— Choose a doctor —</option>
                      {doctors.map(d => (
                        <option key={d.doctorId} value={d.doctorId}>
                          Dr. {d.fullName} — {d.yearsOfExperience} yrs exp — ₹{d.consultationFee}
                        </option>
                      ))}
                    </select>
                    {selectedDoctor && (
                      <div className="doctor-card" style={{ marginTop: '0.75rem' }}>
                        <h4>Dr. {selectedDoctor.fullName}</h4>
                        <p className="spec">{selectedDoctor.specialization}</p>
                        <p className="detail">🏥 License: {selectedDoctor.licenseNumber}</p>
                        <p className="detail">⭐ {selectedDoctor.yearsOfExperience} years experience</p>
                        <p className="fee">₹{selectedDoctor.consultationFee} consultation fee</p>
                      </div>
                    )}
                  </>
                )
              }
            </div>
          )}

          {/* Step 3 — Date */}
          {form.doctorId && (
            <div className="form-group">
              <label>Step 3: Select Date</label>
              <input
                type="date"
                name="appointmentDate"
                value={form.appointmentDate}
                min={today}
                onChange={handleChange}
                required
              />
            </div>
          )}

          {/* Step 4 — Time Slot */}
          {form.appointmentDate && (
            <div className="form-group">
              <label>Step 4: Select Time Slot</label>
              {loadingSlots
                ? <div className="loading" style={{ padding: '1rem' }}><div className="spinner" /></div>
                : slots.length === 0
                  ? <p className="text-warning">⚠️ No available slots for this date. Try another day.</p>
                  : (
                    <div className="slots-grid">
                      {slots.map(slot => (
                        <button
                          key={slot}
                          type="button"
                          className={`slot-btn ${form.appointmentTime === slot ? 'selected' : ''}`}
                          onClick={() => setForm(prev => ({ ...prev, appointmentTime: slot }))}
                        >
                          {slot}
                        </button>
                      ))}
                    </div>
                  )
              }
            </div>
          )}

          {/* Reason */}
          <div className="form-group">
            <label>Reason for Visit (optional)</label>
            <textarea
              name="reasonForVisit"
              value={form.reasonForVisit}
              onChange={handleChange}
              placeholder="Briefly describe your symptoms or reason for visit..."
              rows={3}
            />
          </div>

          <button
            type="submit"
            className="btn btn-primary btn-lg"
            disabled={loading || !form.appointmentTime}
          >
            {loading ? 'Booking...' : '✅ Confirm Appointment'}
          </button>
        </form>
      </div>
    </div>
  );
};

export default BookAppointment;