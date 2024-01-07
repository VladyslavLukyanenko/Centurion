INSERT INTO products.dashboard (id, name, stripe_config_api_key, stripe_config_web_hook_endpoint_secret, owner_id,
                                expires_at, discord_config_bot_access_token, discord_config_oauth_config_authorize_url,
                                logo_src,
                                custom_background_src, time_zone_id, hosting_config_domain_name, hosting_config_mode,
                                charge_backers_export_enabled, created_at, updated_at, updated_by, created_by,
                                removed_at)
VALUES ((select id from products.dashboard limit 1), 'Alantoo''s Dashboard', null, null, 1, null,
        'NzI1NzQyNzczNDQxMTM0NzYz.XwMk2Q.SBdtgIYw4INtZBPk6kOQunQ0E5E',
        'https://discord.com/api/oauth2/authorize?client_id=725742773441134763&redirect_uri=http%3A%2F%2Flocalhost%3A4200%2Faccount%2Foauth2%2Fdiscord%2Fcallback&response_type=code&scope=identify%20email%20guilds.join%20guilds',
        null, null, 'Etc/GMT', 'alantoo', 2, false, '2021-01-19 19:40:56.732346', '2021-01-19 19:40:56.732349', null,
        null, '9999-12-31 23:59:59.999999');


INSERT INTO products.joined_dashboard (dashboard_id, user_id, joined_at)
VALUES ((select id from products.dashboard limit 1), 1, now());

INSERT INTO products.product (id, name, description, images, download_url, image_url, logo_url, icon_url, value,
                              version, discord_role_id, discord_guild_id, checkouts_tracking_webhook_url, features,
                              created_at, updated_at, updated_by, created_by, removed_at, dashboard_id)
VALUES (1, 'Project Raffles',
        'Project Raffles is one of the newest additions to the ProjectIndustries family. ProjectRaffles is easy to use and is an extremely beginner friendly way to get introduced to botting. With Project Raffles constant updates and adapting development',
        ' there is no excuse to not join us.', 'no-url', '/images/raffles-tasks-page.jpg',
        'https://projectindustries.gg/images/logo-project-raffles.png',
        'https://projectindustries.gg/images/logo-project-raffles.png', 'project-raffles', '1.0.0', 728665119780896798,
        696837067547738229, 'no-url',
        '[{"iconUrl":"http://localhost:8080/images/fast-icon.png","title":"Insanely FastXXX","desc":"Request based so you can secure your checkouts before everyone else."},{"iconUrl":"http://localhost:8080/images/auto-spoof-icon.png","title":"Auto SpoofXXX","desc":"Automatically spoof your location to a desired drop zone."},{"iconUrl":"http://localhost:8080/images/unlimited-icon.png","title":"Unlimited TasksXXX","desc":"Using only one emulator, checkout a couple or even hundreds of pairs."},{"iconUrl":"http://localhost:8080/images/support-icon.png","title":"Webhook SupportXXX","desc":"Watch all your checkouts roll in with our Discord webhook support."}]',
        now(), now(), 1, 1, '9999-12-31 23:59:59.999999', (select id from products.dashboard limit 1));

INSERT INTO products.plan (id, product_id, amount, currency, subscription_plan, license_life, trial_period, description,
                           unbindable_delay, is_trial, discord_role_id, protect_purchases_with_captcha, format,
                           template, created_at, updated_at, updated_by, created_by, removed_at, dashboard_id)
VALUES (1, 1, 150, 'USD', 'FAKE_PLAN', '0 years 0 mons 60 days 0 hours 0 mins 0.00 secs',
        '0 years 0 mons 30 days 0 hours 0 mins 0.00 secs', 'Renew 50 USD', null, false, 729370902315270185, false, 1,
        null, now(), now(), 1, 1, '9999-12-31 23:59:59.999999', (select id from products.dashboard limit 1));


INSERT INTO products.release (id, password, initial_stock, type, stock, title, plan_id, concurrency_stamp,
                              created_at, updated_at, updated_by, created_by, removed_at, dashboard_id)
VALUES (1, 'abc', 150, 0, 6, 'January Restock', 1, 'adadasdqe11122112fr1f', now(), now(), 1, 1,
        '9999-12-31 23:59:59.999999', (select id from products.dashboard limit 1));

INSERT INTO products.license_key (id, user_id, plan_id, product_id, release_id, expiry, last_auth_request, session_id,
                                  subscription_id, subscribed_at, subscription_cancelled_at, payment_intent, value,
                                  reason, trial_ends_at, unbindable_after, suspensions, created_at, updated_at,
                                  updated_by,
                                  created_by, removed_at, dashboard_id)
VALUES (1, 1, 1, 1, 1, '2021-01-19 19:40:56.732346', null, null, 'FAKE_PLAN', '2021-01-19 19:40:56.732346', null, null,
        '3b007573-6166-4574-9248-753e491abf09', 'Demo data', null, null, '[]', now(), now(), 1, 1,
        '9999-12-31 23:59:59.999999', (select id from products.dashboard limit 1));


INSERT INTO security.member_role (id, name, permissions, salary, currency, payout_frequency, created_at, updated_at,
                                  updated_by, created_by, removed_at, dashboard_id)
VALUES (1, 'Admin', '[]', 0, 1, 3, now(), now(), 1, 1, '9999-12-31 23:59:59.999999',
        (select id from products.dashboard limit 1));

INSERT INTO security.user_member_role_binding (id, member_role_id, user_id, last_paid_out_at, remote_account_id,
                                               dashboard_id, created_at, updated_at, updated_by, created_by)
VALUES (1, 1, 1, null, 'no-acc', (select id from products.dashboard limit 1), now(), now(), 1, 1);
