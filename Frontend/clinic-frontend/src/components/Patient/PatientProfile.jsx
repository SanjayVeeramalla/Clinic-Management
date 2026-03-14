import React, { useEffect, useState } from 'react';
import { patientAPI } from '../../services/api';
import toast from 'react-hot-toast';

const BLOOD_GROUPS = ['A+', 'A-', 'B+', 'B-', 'AB+', 'AB-', 'O+', 'O-'];

const PatientProfile = () => {
  const [profile, setProfile] = useState(null);
  const [form, setForm]       = useState({});
  const [loading, setLoading] = useState(true);
  const [saving, setSaving]   = useState(false);
  const [editing, setEditing] = useState(false);

  useEffect(() => {
    patientAPI.getProfile()
      .then(res => {
        const p = res.data.data;
        setProfile(p);
        setForm({
          fullName:        p.fullName        || '',
          phone:           p.phone           || '',
          dateOfBirth:     p.dateOfBirth     || '',
          gender:          p.gender          || '',
          bloodGroup:      p.bloodGroup      || '',
          address:         p.address         || '',
          emergencyContact:p.emergencyContact|| '',
          medicalHistory:  p.medicalHistory  || '',
        });
      })
      .catch(() => toast.error('Failed to load profile'))
      .finally(() => setLoading(false));
  }, []);

  const handleChange = (e) =>
    setForm(prev => ({ ...prev, [e.target.name]: e.target.value }));

  const handleSave = async (e) => {
    e.preventDefault();
    setSaving(true);
    try {
      await patientAPI.updateProfile(form);
      toast.success('Profile updated successfully!');
      setEditing(false);
      setProfile(prev => ({ ...prev, ...form }));
    } catch (err) {
      toast.error(err.response?.data?.message || 'Update failed');
    } finally {
      setSaving(false);
    }
  };

  if (loading) return <div className="loading"><div className="spinner" /></div>;

  return (
    <div>
      <div className="page-header">
        <div>
          <h1>My Profile</h1>
          <p className="text-muted">Manage your personal and medical information</p>
        </div>
        {!editing && (
          <button className="btn btn-primary" onClick={() => setEditing(true)}>✏️ Edit Profile</button>
        )}
      </div>

      <div className="form-card" style={{ maxWidth: '700px' }}>
        {!editing ? (
          /* View mode */
          <div style={{ display: 'grid', gap: '1rem' }}>
            {[
              ['Full Name',         profile?.fullName],
              ['Email',             profile?.email],
              ['Phone',             profile?.phone || '—'],
              ['Date of Birth',     profile?.dateOfBirth || '—'],
              ['Gender',            profile?.gender || '—'],
              ['Blood Group',       profile?.bloodGroup || '—'],
              ['Address',           profile?.address || '—'],
              ['Emergency Contact', profile?.emergencyContact || '—'],
              ['Medical History',   profile?.medicalHistory || '—'],
            ].map(([label, value]) => (
              <div key={label} style={{ display: 'flex', gap: '1rem' }}>
                <span style={{ width: '160px', fontWeight: 600, color: '#475569', flexShrink: 0 }}>{label}</span>
                <span style={{ color: '#1e293b' }}>{value}</span>
              </div>
            ))}
          </div>
        ) : (
          /* Edit mode */
          <form onSubmit={handleSave} style={{ display: 'flex', flexDirection: 'column', gap: '1.25rem' }}>
            <div className="form-row">
              <div className="form-group">
                <label>Full Name *</label>
                <input name="fullName" value={form.fullName} onChange={handleChange} required />
              </div>
              <div className="form-group">
                <label>Phone</label>
                <input name="phone" value={form.phone} onChange={handleChange} placeholder="9XXXXXXXXX" />
              </div>
            </div>

            <div className="form-row">
              <div className="form-group">
                <label>Date of Birth</label>
                <input type="date" name="dateOfBirth" value={form.dateOfBirth} onChange={handleChange} />
              </div>
              <div className="form-group">
                <label>Gender</label>
                <select name="gender" value={form.gender} onChange={handleChange}>
                  <option value="">— Select —</option>
                  <option value="Male">Male</option>
                  <option value="Female">Female</option>
                  <option value="Other">Other</option>
                </select>
              </div>
            </div>

            <div className="form-row">
              <div className="form-group">
                <label>Blood Group</label>
                <select name="bloodGroup" value={form.bloodGroup} onChange={handleChange}>
                  <option value="">— Select —</option>
                  {BLOOD_GROUPS.map(g => <option key={g} value={g}>{g}</option>)}
                </select>
              </div>
              <div className="form-group">
                <label>Emergency Contact</label>
                <input name="emergencyContact" value={form.emergencyContact} onChange={handleChange} placeholder="9XXXXXXXXX" />
              </div>
            </div>

            <div className="form-group">
              <label>Address</label>
              <input name="address" value={form.address} onChange={handleChange} placeholder="Street, City, State" />
            </div>

            <div className="form-group">
              <label>Medical History / Allergies</label>
              <textarea name="medicalHistory" value={form.medicalHistory} onChange={handleChange} rows={4}
                placeholder="List any known conditions, allergies, or previous surgeries..." />
            </div>

            <div style={{ display: 'flex', gap: '0.75rem', justifyContent: 'flex-end' }}>
              <button type="button" className="btn btn-ghost" onClick={() => setEditing(false)}>Cancel</button>
              <button type="submit" className="btn btn-primary" disabled={saving}>
                {saving ? 'Saving...' : '💾 Save Changes'}
              </button>
            </div>
          </form>
        )}
      </div>
    </div>
  );
};

export default PatientProfile;