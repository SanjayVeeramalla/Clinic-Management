import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { doctorAPI, patientAPI } from '../../services/api';
import toast from 'react-hot-toast';

const PrescriptionForm = () => {
  const { appointmentId } = useParams();
  const navigate = useNavigate();

  const [appointment, setAppointment] = useState(null);
  const [loading, setLoading]         = useState(true);
  const [saving, setSaving]           = useState(false);
  const [form, setForm] = useState({
    diagnosis:    '',
    medications:  '',
    instructions: '',
    followUpDate: '',
  });

  useEffect(() => {
    patientAPI.getAppointment(appointmentId)
      .then(res => {
        const apt = res.data.data;
        setAppointment(apt);
        // Prefill if prescription exists
        if (apt.prescription) {
          setForm({
            diagnosis:    apt.prescription.diagnosis    || '',
            medications:  apt.prescription.medications  || '',
            instructions: apt.prescription.instructions || '',
            followUpDate: apt.prescription.followUpDate || '',
          });
        }
      })
      .catch(() => toast.error('Failed to load appointment'))
      .finally(() => setLoading(false));
  }, [appointmentId]);

  const handleChange = (e) =>
    setForm(prev => ({ ...prev, [e.target.name]: e.target.value }));

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!form.diagnosis.trim())   { toast.error('Diagnosis is required'); return; }
    if (!form.medications.trim()) { toast.error('Medications are required'); return; }

    setSaving(true);
    try {
      await doctorAPI.addPrescription(appointmentId, form);
      toast.success('Prescription saved successfully!');
      navigate('/doctor/appointments');
    } catch (err) {
      toast.error(err.response?.data?.message || 'Failed to save prescription');
    } finally {
      setSaving(false);
    }
  };

  if (loading) return <div className="loading"><div className="spinner" /></div>;

  return (
    <div>
      <div className="page-header">
        <div>
          <h1>💊 Write Prescription</h1>
          {appointment && (
            <p className="text-muted">
              Patient: {appointment.patientName} —{' '}
              {new Date(appointment.appointmentDate).toLocaleDateString('en-IN')} at {appointment.appointmentTime?.slice(0,5)}
            </p>
          )}
        </div>
        <button className="btn btn-ghost" onClick={() => navigate(-1)}>← Back</button>
      </div>

      {appointment && (
        <div className="card" style={{ maxWidth: '700px' }}>
          <div style={{ display: 'flex', gap: '2rem', marginBottom: '1.5rem' }}>
            <div>
              <p className="text-muted" style={{ fontSize: '0.8rem' }}>PATIENT</p>
              <p style={{ fontWeight: 700 }}>{appointment.patientName}</p>
            </div>
            <div>
              <p className="text-muted" style={{ fontSize: '0.8rem' }}>DATE</p>
              <p style={{ fontWeight: 700 }}>{new Date(appointment.appointmentDate).toLocaleDateString('en-IN')}</p>
            </div>
            <div>
              <p className="text-muted" style={{ fontSize: '0.8rem' }}>REASON</p>
              <p style={{ fontWeight: 700 }}>{appointment.reasonForVisit || '—'}</p>
            </div>
          </div>
        </div>
      )}

      <div className="form-card">
        <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '1.25rem' }}>
          <div className="form-group">
            <label>Diagnosis *</label>
            <textarea name="diagnosis" value={form.diagnosis} onChange={handleChange}
              rows={2} placeholder="Primary diagnosis..." required />
          </div>

          <div className="form-group">
            <label>Medications *</label>
            <textarea name="medications" value={form.medications} onChange={handleChange}
              rows={4} placeholder="e.g. Paracetamol 500mg — 1 tablet twice daily for 5 days&#10;Amoxicillin 250mg — 1 capsule three times daily for 7 days" required />
          </div>

          <div className="form-group">
            <label>Instructions / Advice</label>
            <textarea name="instructions" value={form.instructions} onChange={handleChange}
              rows={3} placeholder="e.g. Take medications after meals. Avoid cold drinks. Rest for 2 days." />
          </div>

          <div className="form-group">
            <label>Follow-up Date (optional)</label>
            <input type="date" name="followUpDate" value={form.followUpDate} onChange={handleChange}
              min={new Date().toISOString().split('T')[0]} />
          </div>

          <div style={{ display: 'flex', gap: '0.75rem', justifyContent: 'flex-end' }}>
            <button type="button" className="btn btn-ghost" onClick={() => navigate(-1)}>Cancel</button>
            <button type="submit" className="btn btn-primary" disabled={saving}>
              {saving ? 'Saving...' : '💾 Save Prescription'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default PrescriptionForm;