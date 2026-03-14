import React, { useEffect, useState } from 'react';
import { doctorAPI } from '../../services/api';
import toast from 'react-hot-toast';

const DAYS = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];

const defaultSlot = { startTime: '09:00', endTime: '17:00', slotDurationMinutes: 30, active: false };

const DoctorSchedule = () => {
  const [profile, setProfile]     = useState(null);
  const [schedules, setSchedules] = useState(
    DAYS.map((_, i) => ({ dayOfWeek: i, ...defaultSlot }))
  );
  const [saving, setSaving] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    doctorAPI.getProfile()
      .then(res => setProfile(res.data.data))
      .catch(() => toast.error('Failed to load profile'))
      .finally(() => setLoading(false));
  }, []);

  const updateDay = (dayOfWeek, field, value) => {
    setSchedules(prev => prev.map(s =>
      s.dayOfWeek === dayOfWeek ? { ...s, [field]: value } : s
    ));
  };

  const handleSave = async (day) => {
    if (!profile) return;
    setSaving(day.dayOfWeek);
    try {
      await doctorAPI.setSchedule(profile.doctorId, {
        dayOfWeek:          day.dayOfWeek,
        startTime:          day.startTime,
        endTime:            day.endTime,
        slotDurationMinutes: day.slotDurationMinutes,
      });
      updateDay(day.dayOfWeek, 'active', true);
      toast.success(`${DAYS[day.dayOfWeek]} schedule saved!`);
    } catch (err) {
      toast.error(err.response?.data?.message || 'Failed to save schedule');
    } finally {
      setSaving(null);
    }
  };

  if (loading) return <div className="loading"><div className="spinner" /></div>;

  return (
    <div>
      <div className="page-header">
        <div>
          <h1>My Schedule</h1>
          <p className="text-muted">Set your working hours for each day of the week</p>
        </div>
      </div>

      <div className="schedule-grid">
        {schedules.map(day => (
          <div key={day.dayOfWeek} className={`schedule-day ${day.active ? 'active' : ''}`}>
            <h4>{day.active ? '✅ ' : ''}{DAYS[day.dayOfWeek]}</h4>

            <div className="form-group mb-1">
              <label>Start Time</label>
              <input type="time" value={day.startTime}
                onChange={e => updateDay(day.dayOfWeek, 'startTime', e.target.value)} />
            </div>

            <div className="form-group mb-1">
              <label>End Time</label>
              <input type="time" value={day.endTime}
                onChange={e => updateDay(day.dayOfWeek, 'endTime', e.target.value)} />
            </div>

            <div className="form-group mb-2">
              <label>Slot Duration</label>
              <select value={day.slotDurationMinutes}
                onChange={e => updateDay(day.dayOfWeek, 'slotDurationMinutes', parseInt(e.target.value))}>
                <option value={15}>15 minutes</option>
                <option value={20}>20 minutes</option>
                <option value={30}>30 minutes</option>
                <option value={45}>45 minutes</option>
                <option value={60}>60 minutes</option>
              </select>
            </div>

            <button
              className="btn btn-primary btn-sm btn-full"
              onClick={() => handleSave(day)}
              disabled={saving === day.dayOfWeek}
            >
              {saving === day.dayOfWeek ? 'Saving...' : 'Save'}
            </button>
          </div>
        ))}
      </div>
    </div>
  );
};

export default DoctorSchedule;