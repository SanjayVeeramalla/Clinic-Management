import React, { useState } from 'react';
import { adminAPI } from '../../services/api';
import toast from 'react-hot-toast';

const today = new Date().toISOString().split('T')[0];
const monthAgo = new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0];

const Reports = () => {
  const [activeReport, setActiveReport] = useState('appointments');
  const [from, setFrom]   = useState(monthAgo);
  const [to, setTo]       = useState(today);
  const [data, setData]   = useState(null);
  const [loading, setLoading] = useState(false);

  const runReport = async () => {
    if (!from || !to) { toast.error('Select date range'); return; }
    if (from > to)    { toast.error('From date must be before To date'); return; }

    setLoading(true);
    setData(null);
    try {
      const payload = { fromDate: from, toDate: to };
      const res = activeReport === 'appointments'
        ? await adminAPI.appointmentSummaryReport(payload)
        : await adminAPI.doctorWorkloadReport(payload);
      setData(res.data.data || []);
    } catch (err) {
      toast.error(err.response?.data?.message || 'Report generation failed');
    } finally {
      setLoading(false);
    }
  };

  const totalAppts = data && activeReport === 'appointments'
    ? data.reduce((s, r) => s + r.count, 0) : 0;

  return (
    <div>
      <div className="page-header">
        <div>
          <h1>Reports & Analytics</h1>
          <p className="text-muted">Generate reports for any date range</p>
        </div>
      </div>

      {/* Report type tabs */}
      <div className="tabs">
        <button className={`tab ${activeReport === 'appointments' ? 'active' : ''}`}
          onClick={() => { setActiveReport('appointments'); setData(null); }}>
          📊 Appointment Summary
        </button>
        <button className={`tab ${activeReport === 'workload' ? 'active' : ''}`}
          onClick={() => { setActiveReport('workload'); setData(null); }}>
          👨‍⚕️ Doctor Workload
        </button>
      </div>

      {/* Date range filter */}
      <div className="card" style={{ marginBottom: '1.5rem' }}>
        <div className="filter-row">
          <div className="form-group" style={{ flexDirection: 'row', alignItems: 'center', gap: '0.5rem' }}>
            <label style={{ whiteSpace: 'nowrap', fontWeight: 600 }}>From:</label>
            <input type="date" value={from} onChange={e => setFrom(e.target.value)}
              style={{ padding: '0.6rem', border: '1.5px solid #d1d5db', borderRadius: '8px' }} />
          </div>
          <div className="form-group" style={{ flexDirection: 'row', alignItems: 'center', gap: '0.5rem' }}>
            <label style={{ whiteSpace: 'nowrap', fontWeight: 600 }}>To:</label>
            <input type="date" value={to} onChange={e => setTo(e.target.value)}
              style={{ padding: '0.6rem', border: '1.5px solid #d1d5db', borderRadius: '8px' }} />
          </div>
          <button className="btn btn-primary" onClick={runReport} disabled={loading}>
            {loading ? 'Generating...' : '📊 Generate Report'}
          </button>
        </div>
      </div>

      {/* Results */}
      {loading && <div className="loading"><div className="spinner" /></div>}

      {data && activeReport === 'appointments' && (
        <div className="card">
          <div className="card-header">
            <h2>Appointment Summary</h2>
            <span className="text-muted">{from} → {to} · Total: {totalAppts}</span>
          </div>

          {data.length === 0 ? (
            <div className="empty-state"><span className="empty-icon">📭</span><h3>No data for this period</h3></div>
          ) : (
            <>
              <div className="report-summary">
                {data.map(row => (
                  <div key={row.status} className="report-stat">
                    <h4>{row.count}</h4>
                    <p>{row.status}</p>
                    <p style={{ fontSize: '0.75rem', color: '#94a3b8' }}>{row.percentage?.toFixed(1)}%</p>
                  </div>
                ))}
              </div>

              {/* Simple bar chart */}
              <div style={{ marginTop: '1.5rem' }}>
                {data.map(row => (
                  <div key={row.status} style={{ marginBottom: '0.75rem' }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '0.25rem', fontSize: '0.875rem' }}>
                      <span style={{ fontWeight: 600 }}>{row.status}</span>
                      <span className="text-muted">{row.count} ({row.percentage?.toFixed(1)}%)</span>
                    </div>
                    <div style={{ background: '#f1f5f9', borderRadius: '4px', height: '12px', overflow: 'hidden' }}>
                      <div style={{
                        width: `${row.percentage}%`,
                        height: '100%',
                        background: row.status === 'Completed' ? '#10b981'
                          : row.status === 'Pending'   ? '#f59e0b'
                          : row.status === 'Confirmed' ? '#3b82f6'
                          : '#ef4444',
                        transition: 'width 0.5s ease'
                      }} />
                    </div>
                  </div>
                ))}
              </div>
            </>
          )}
        </div>
      )}

      {data && activeReport === 'workload' && (
        <div className="card">
          <div className="card-header">
            <h2>Doctor Workload Report</h2>
            <span className="text-muted">{from} → {to}</span>
          </div>

          {data.length === 0 ? (
            <div className="empty-state"><span className="empty-icon">📭</span><h3>No data for this period</h3></div>
          ) : (
            <div className="table-wrapper">
              <table>
                <thead>
                  <tr>
                    <th>Doctor</th>
                    <th>Specialization</th>
                    <th>Total</th>
                    <th>Completed</th>
                    <th>Cancelled</th>
                    <th>Pending</th>
                    <th>Completion Rate</th>
                  </tr>
                </thead>
                <tbody>
                  {data.map(d => {
                    const rate = d.totalAppointments > 0
                      ? ((d.completed / d.totalAppointments) * 100).toFixed(0) : 0;
                    return (
                      <tr key={d.doctorId}>
                        <td style={{ fontWeight: 600 }}>Dr. {d.doctorName}</td>
                        <td>{d.specialization}</td>
                        <td style={{ fontWeight: 700 }}>{d.totalAppointments}</td>
                        <td><span className="badge badge-completed">{d.completed}</span></td>
                        <td><span className="badge badge-cancelled">{d.cancelled}</span></td>
                        <td><span className="badge badge-pending">{d.pending}</span></td>
                        <td>
                          <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                            <div style={{ flex: 1, background: '#f1f5f9', borderRadius: '4px', height: '8px' }}>
                              <div style={{ width: `${rate}%`, height: '100%', background: '#10b981', borderRadius: '4px' }} />
                            </div>
                            <span style={{ fontSize: '0.8rem', fontWeight: 600 }}>{rate}%</span>
                          </div>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}
    </div>
  );
};

export default Reports;