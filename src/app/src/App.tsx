import { Routes, Route, Navigate } from 'react-router-dom';
import AppShell from './components/layout/AppShell';
import DashboardPage from './pages/DashboardPage';
import TimesheetPage from './pages/TimesheetPage';
import TimeEntryListPage from './pages/TimeEntryListPage';
import TimeEntryFormPage from './pages/TimeEntryFormPage';
import UnitListPage from './pages/UnitListPage';
import UnitDetailPage from './pages/UnitDetailPage';
import CustomerListPage from './pages/CustomerListPage';
import CustomerDetailPage from './pages/CustomerDetailPage';
import ProjectListPage from './pages/ProjectListPage';
import ProjectDetailPage from './pages/ProjectDetailPage';
import TaskDetailPage from './pages/TaskDetailPage';
import UserListPage from './pages/UserListPage';
import ReportsPage from './pages/ReportsPage';
import ProjectReportPage from './pages/ProjectReportPage';
import UserReportPage from './pages/UserReportPage';
import OverallReportPage from './pages/OverallReportPage';
import ProfilePage from './pages/ProfilePage';

export default function App() {
  return (
    <AppShell>
      <Routes>
        <Route path="/" element={<Navigate to="/dashboard" replace />} />
        <Route path="/dashboard" element={<DashboardPage />} />
        <Route path="/timesheet" element={<TimesheetPage />} />
        <Route path="/time-entries" element={<TimeEntryListPage />} />
        <Route path="/time-entries/new" element={<TimeEntryFormPage />} />
        <Route path="/time-entries/:id/edit" element={<TimeEntryFormPage />} />
        <Route path="/units" element={<UnitListPage />} />
        <Route path="/units/:id" element={<UnitDetailPage />} />
        <Route path="/customers" element={<CustomerListPage />} />
        <Route path="/customers/:id" element={<CustomerDetailPage />} />
        <Route path="/projects" element={<ProjectListPage />} />
        <Route path="/projects/:id" element={<ProjectDetailPage />} />
        <Route path="/projects/:projectId/tasks/:id" element={<TaskDetailPage />} />
        <Route path="/admin/users" element={<UserListPage />} />
        <Route path="/reports" element={<ReportsPage />} />
        <Route path="/reports/project/:id" element={<ProjectReportPage />} />
        <Route path="/reports/user/:id" element={<UserReportPage />} />
        <Route path="/reports/overall" element={<OverallReportPage />} />
        <Route path="/profile" element={<ProfilePage />} />
      </Routes>
    </AppShell>
  );
}
